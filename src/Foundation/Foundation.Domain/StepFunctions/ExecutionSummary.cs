namespace Foundation.Domain.StepFunctions;

/// <summary>
/// A concise view of a Step Functions execution as it appears in a list.
/// </summary>
/// <param name="ExecutionArn">The Amazon Resource Name that uniquely identifies the execution.</param>
/// <param name="Name">The execution name.</param>
/// <param name="StateMachineArn">The Amazon Resource Name of the state machine the execution belongs to.</param>
/// <param name="Status">The execution status, for example <c>RUNNING</c>, <c>SUCCEEDED</c>, or <c>FAILED</c>.</param>
/// <param name="StartDate">The moment the execution started.</param>
/// <param name="StopDate">The moment the execution stopped, or <see langword="null"/> when it is still running.</param>
public sealed record ExecutionSummary(
    string ExecutionArn,
    string Name,
    string StateMachineArn,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset? StopDate);
