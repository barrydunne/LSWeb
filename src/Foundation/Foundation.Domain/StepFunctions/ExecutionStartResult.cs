namespace Foundation.Domain.StepFunctions;

/// <summary>
/// The result of starting a Step Functions execution.
/// </summary>
/// <param name="ExecutionArn">The Amazon Resource Name that uniquely identifies the new execution.</param>
/// <param name="StartDate">The moment the execution started.</param>
public sealed record ExecutionStartResult(
    string ExecutionArn,
    DateTimeOffset StartDate);
