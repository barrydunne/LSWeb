namespace Foundation.Api.Models;

/// <summary>
/// The in-session activity log of completed backend operations.
/// </summary>
/// <param name="Entries">The recorded activity entries, most recent first.</param>
public sealed record ActivityResponse(IReadOnlyList<ActivityEntryResponse> Entries);

/// <summary>
/// A single recorded backend operation outcome.
/// </summary>
/// <param name="OperationId">The identifier shared with the operation's notifications.</param>
/// <param name="Operation">The name of the operation, for example <c>catalogue-refresh</c>.</param>
/// <param name="State">The terminal lifecycle state: <c>InProgress</c>, <c>Succeeded</c>, or <c>Failed</c>.</param>
/// <param name="Message">A human-readable message describing the outcome.</param>
/// <param name="OccurredAt">When the entry was recorded.</param>
public sealed record ActivityEntryResponse(
    string OperationId,
    string Operation,
    string State,
    string Message,
    DateTimeOffset OccurredAt);
