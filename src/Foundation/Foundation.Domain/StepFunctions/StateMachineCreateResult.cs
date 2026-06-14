namespace Foundation.Domain.StepFunctions;

/// <summary>
/// The result of creating a Step Functions state machine.
/// </summary>
/// <param name="StateMachineArn">The Amazon Resource Name that uniquely identifies the new state machine.</param>
/// <param name="CreationDate">The moment the state machine was created.</param>
public sealed record StateMachineCreateResult(
    string StateMachineArn,
    DateTimeOffset CreationDate);
