using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Preferences;
using Foundation.Domain.Preferences;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Preferences;

/// <summary>
/// Persists user preferences as a JSON document under the mounted host directory so they survive
/// container restarts. Read and write failures are surfaced as a <see cref="Result"/> rather than
/// thrown so callers never have to catch exceptions across layers.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed partial class FileUserDataStore : IUserDataStore
{
    private const string FileName = "preferences.json";

    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    private readonly string _directory;
    private readonly string _filePath;
    private readonly ILogger _logger;

    public FileUserDataStore(UserDataSettings settings, ILogger<FileUserDataStore> logger)
    {
        _directory = settings.DataDirectory!;
        _filePath = Path.Combine(_directory, FileName);
        _logger = logger;
    }

    public async Task<Result<UserPreferences>> GetPreferencesAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
            return UserPreferences.Empty;

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var preferences = await JsonSerializer.DeserializeAsync<UserPreferences>(stream, _serializerOptions, cancellationToken);
            return preferences ?? UserPreferences.Empty;
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            LogReadFailed(_filePath, exception.Message);
            return new Error($"Could not read user preferences: {exception.Message}");
        }
    }

    public async Task<Result> SavePreferencesAsync(UserPreferences preferences, CancellationToken cancellationToken)
    {
        try
        {
            Directory.CreateDirectory(_directory);
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, preferences, _serializerOptions, cancellationToken);
            return Result.Success();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            LogWriteFailed(_filePath, exception.Message);
            return new Error($"Could not save user preferences: {exception.Message}");
        }
    }

    [LoggerMessage(LogLevel.Warning, "Failed to read user preferences from {FilePath}: {ErrorMessage}")]
    private partial void LogReadFailed(string filePath, string errorMessage);

    [LoggerMessage(LogLevel.Warning, "Failed to save user preferences to {FilePath}: {ErrorMessage}")]
    private partial void LogWriteFailed(string filePath, string errorMessage);
}
