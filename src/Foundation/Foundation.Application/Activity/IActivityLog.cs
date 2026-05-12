using Foundation.Domain.Activity;

namespace Foundation.Application.Activity;

/// <summary>
/// Records and exposes an in-session history of completed backend operations. The application layer
/// appends entries through this abstraction without depending on how the log is stored.
/// </summary>
public interface IActivityLog
{
    /// <summary>
    /// Append a completed-operation entry to the log.
    /// </summary>
    /// <param name="entry">The entry to record.</param>
    void Append(ActivityEntry entry);

    /// <summary>
    /// Get the recorded entries, most recent first.
    /// </summary>
    /// <returns>The recorded entries ordered newest to oldest.</returns>
    IReadOnlyList<ActivityEntry> GetEntries();
}
