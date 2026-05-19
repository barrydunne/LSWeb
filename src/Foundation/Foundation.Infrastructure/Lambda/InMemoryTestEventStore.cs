using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Domain.Lambda;

namespace Foundation.Infrastructure.Lambda;

/// <summary>
/// Keeps named Lambda test events in process memory only. Used when no host directory is mounted so
/// the container remains stateless; the data is lost when the process stops.
/// </summary>
internal sealed class InMemoryTestEventStore : ITestEventStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, List<LambdaTestEvent>> _events = new(StringComparer.Ordinal);

    public Task<Result<IReadOnlyList<LambdaTestEvent>>> GetEventsAsync(string functionName, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            IReadOnlyList<LambdaTestEvent> events = _events.TryGetValue(functionName, out var stored)
                ? [.. stored]
                : [];
            return Task.FromResult(Ok(events));
        }
    }

    public Task<Result> SaveEventAsync(string functionName, LambdaTestEvent testEvent, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (!_events.TryGetValue(functionName, out var stored))
            {
                stored = [];
                _events[functionName] = stored;
            }

            stored.RemoveAll(_ => string.Equals(_.Name, testEvent.Name, StringComparison.Ordinal));
            stored.Add(testEvent);
        }

        return Task.FromResult(Result.Success());
    }

    public Task<Result> DeleteEventAsync(string functionName, string name, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_events.TryGetValue(functionName, out var stored))
                stored.RemoveAll(_ => string.Equals(_.Name, name, StringComparison.Ordinal));
        }

        return Task.FromResult(Result.Success());
    }

    private static Result<T> Ok<T>(T value) => value;
}
