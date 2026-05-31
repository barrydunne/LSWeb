namespace Foundation.Domain.StepFunctions;

/// <summary>
/// A single event in the history of a Step Functions execution, normalised so the timeline can show
/// the state name and any input, output, error, or cause regardless of the underlying event type.
/// </summary>
/// <param name="Id">The identifier of the event within the execution history.</param>
/// <param name="PreviousEventId">The identifier of the preceding event, or <see langword="null"/> for the first event.</param>
/// <param name="Type">The event type, for example <c>TaskStateEntered</c> or <c>ExecutionSucceeded</c>.</param>
/// <param name="Timestamp">The moment the event occurred.</param>
/// <param name="Name">The state name associated with the event, where one applies.</param>
/// <param name="Input">The JSON input recorded for the event, where one applies.</param>
/// <param name="Output">The JSON output recorded for the event, where one applies.</param>
/// <param name="Error">The error name recorded for a failure event, where one applies.</param>
/// <param name="Cause">The failure cause recorded for a failure event, where one applies.</param>
public sealed record ExecutionHistoryEvent(
    long Id,
    long? PreviousEventId,
    string Type,
    DateTimeOffset Timestamp,
    string? Name,
    string? Input,
    string? Output,
    string? Error,
    string? Cause);
