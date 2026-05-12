using Foundation.Application.Activity;
using Foundation.Domain.Activity;

namespace Foundation.Infrastructure.Activity;

/// <summary>
/// Thread-safe in-memory activity log retaining the most recent operation entries. The log is
/// bounded to a fixed capacity so the console's memory footprint stays constant, and it is
/// discarded when the process restarts so statelessness is preserved.
/// </summary>
internal sealed class ActivityLog : IActivityLog
{
    private const int Capacity = 100;

    private readonly object _gate = new();
    private readonly LinkedList<ActivityEntry> _entries = new();

    public void Append(ActivityEntry entry)
    {
        lock (_gate)
        {
            _entries.AddFirst(entry);
            if (_entries.Count > Capacity)
            {
                _entries.RemoveLast();
            }
        }
    }

    public IReadOnlyList<ActivityEntry> GetEntries()
    {
        lock (_gate)
        {
            return _entries.ToArray();
        }
    }
}
