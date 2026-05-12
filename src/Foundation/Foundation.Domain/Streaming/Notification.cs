namespace Foundation.Domain.Streaming;

/// <summary>
/// A real-time message describing the progress or outcome of a backend operation. Notifications
/// are pushed to connected clients so the user always knows the result of an action.
/// </summary>
/// <param name="OperationId">A stable identifier shared by every notification for one operation.</param>
/// <param name="Operation">The name of the operation, for example <c>catalogue-refresh</c>.</param>
/// <param name="State">The lifecycle state being reported.</param>
/// <param name="Message">A human-readable message safe to show to the user, including AWS error detail.</param>
/// <param name="OccurredAt">When the notification was raised.</param>
public sealed record Notification(
    string OperationId,
    string Operation,
    OperationState State,
    string Message,
    DateTimeOffset OccurredAt);
