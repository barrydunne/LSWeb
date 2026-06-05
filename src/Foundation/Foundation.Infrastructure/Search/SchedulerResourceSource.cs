using Foundation.Application.Scheduler;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes EventBridge Scheduler schedules to the global search index. Failures are swallowed
/// and reported as an empty list so a backend that is unavailable or unsupported cannot abort a full
/// index rebuild.
/// </summary>
internal sealed class SchedulerResourceSource : IResourceSource
{
    private readonly ISchedulerClient _client;

    public SchedulerResourceSource(ISchedulerClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "scheduler";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var schedules = await _client.ListSchedulesAsync(cancellationToken);
        if (!schedules.IsSuccess)
        {
            return [];
        }

        return schedules.Value
            .Select(schedule => new SearchEntry(
                ServiceKey,
                schedule.Name,
                schedule.Name,
                $"/services/scheduler/{Uri.EscapeDataString($"{schedule.GroupName}/{schedule.Name}")}"))
            .ToList();
    }
}
