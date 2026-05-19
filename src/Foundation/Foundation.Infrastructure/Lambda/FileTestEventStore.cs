using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Domain.Lambda;
using Foundation.Infrastructure.Preferences;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Lambda;

/// <summary>
/// Persists named Lambda test events as a JSON document under the mounted host directory so they
/// survive container restarts. Read and write failures are surfaced as a <see cref="Result"/>
/// rather than thrown so callers never have to catch exceptions across layers.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed partial class FileTestEventStore : ITestEventStore
{
    private const string FileName = "lambda-test-events.json";

    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    private readonly string _directory;
    private readonly string _filePath;
    private readonly ILogger _logger;

    public FileTestEventStore(UserDataSettings settings, ILogger<FileTestEventStore> logger)
    {
        _directory = settings.DataDirectory!;
        _filePath = Path.Combine(_directory, FileName);
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<LambdaTestEvent>>> GetEventsAsync(string functionName, CancellationToken cancellationToken)
    {
        var all = await ReadAllAsync(cancellationToken);
        if (!all.IsSuccess)
        {
            Result<IReadOnlyList<LambdaTestEvent>> failure = all.Error!.Value;
            return failure;
        }

        IReadOnlyList<LambdaTestEvent> events = all.Value.TryGetValue(functionName, out var stored)
            ? stored
            : [];
        return Ok(events);
    }

    public async Task<Result> SaveEventAsync(string functionName, LambdaTestEvent testEvent, CancellationToken cancellationToken)
    {
        var all = await ReadAllAsync(cancellationToken);
        if (!all.IsSuccess)
            return all.Error!.Value;

        var map = all.Value;
        if (!map.TryGetValue(functionName, out var stored))
        {
            stored = [];
            map[functionName] = stored;
        }

        stored.RemoveAll(_ => string.Equals(_.Name, testEvent.Name, StringComparison.Ordinal));
        stored.Add(testEvent);

        return await WriteAllAsync(map, cancellationToken);
    }

    public async Task<Result> DeleteEventAsync(string functionName, string name, CancellationToken cancellationToken)
    {
        var all = await ReadAllAsync(cancellationToken);
        if (!all.IsSuccess)
            return all.Error!.Value;

        var map = all.Value;
        if (map.TryGetValue(functionName, out var stored))
            stored.RemoveAll(_ => string.Equals(_.Name, name, StringComparison.Ordinal));

        return await WriteAllAsync(map, cancellationToken);
    }

    private async Task<Result<Dictionary<string, List<LambdaTestEvent>>>> ReadAllAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
            return new Dictionary<string, List<LambdaTestEvent>>(StringComparer.Ordinal);

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var map = await JsonSerializer.DeserializeAsync<Dictionary<string, List<LambdaTestEvent>>>(
                stream, _serializerOptions, cancellationToken);
            return map ?? new Dictionary<string, List<LambdaTestEvent>>(StringComparer.Ordinal);
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            LogReadFailed(_filePath, exception.Message);
            return new Error($"Could not read Lambda test events: {exception.Message}");
        }
    }

    private async Task<Result> WriteAllAsync(Dictionary<string, List<LambdaTestEvent>> map, CancellationToken cancellationToken)
    {
        try
        {
            Directory.CreateDirectory(_directory);
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, map, _serializerOptions, cancellationToken);
            return Result.Success();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            LogWriteFailed(_filePath, exception.Message);
            return new Error($"Could not save Lambda test events: {exception.Message}");
        }
    }

    [LoggerMessage(LogLevel.Warning, "Failed to read Lambda test events from {FilePath}: {ErrorMessage}")]
    private partial void LogReadFailed(string filePath, string errorMessage);

    [LoggerMessage(LogLevel.Warning, "Failed to save Lambda test events to {FilePath}: {ErrorMessage}")]
    private partial void LogWriteFailed(string filePath, string errorMessage);

    private static Result<T> Ok<T>(T value) => value;
}
