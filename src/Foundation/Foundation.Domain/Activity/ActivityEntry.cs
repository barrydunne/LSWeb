using Foundation.Domain.Streaming;

namespace Foundation.Domain.Activity;

/// <summary>
/// A persisted record of a backend operation outcome, retained for the lifetime of the process so
/// that results remain visible after the transient notification that announced them disappears.
/// </summary>
/// <param name="OperationId">The identifier shared with the operation's notifications.</param>
/// <param name="Operation">The name of the operation, for example <c>catalogue-refresh</c>.</param>
/// <param name="State">The terminal lifecycle state recorded for the operation.</param>
/// <param name="Message">A human-readable message describing the outcome, safe to show to the user.</param>
/// <param name="OccurredAt">When the entry was recorded.</param>
public sealed record ActivityEntry(
    string OperationId,
    string Operation,
    OperationState State,
    string Message,
    DateTimeOffset OccurredAt);
