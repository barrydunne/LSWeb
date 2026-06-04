namespace Foundation.Domain.EventBridge;

/// <summary>
/// The outcome of putting a single custom event onto an EventBridge bus.
/// </summary>
/// <param name="EventId">The identifier EventBridge assigned to the accepted event, or <c>null</c> when the entry failed.</param>
/// <param name="FailedEntryCount">The number of entries EventBridge rejected; zero when the event was accepted.</param>
/// <param name="ErrorCode">The error code EventBridge returned for a rejected entry, or <c>null</c> when accepted.</param>
/// <param name="ErrorMessage">The error message EventBridge returned for a rejected entry, or <c>null</c> when accepted.</param>
public sealed record EventBridgePutResult(
    string? EventId,
    int FailedEntryCount,
    string? ErrorCode,
    string? ErrorMessage)
{
    /// <summary>
    /// Gets a value indicating whether EventBridge accepted the event.
    /// </summary>
    public bool Accepted => FailedEntryCount == 0;
}
