namespace Foundation.Domain.StepFunctions;

/// <summary>
/// A concise view of a Step Functions state machine as it appears in a list.
/// </summary>
/// <param name="Name">The state machine name.</param>
/// <param name="StateMachineArn">The Amazon Resource Name that uniquely identifies the state machine.</param>
/// <param name="Type">The state machine type, either <c>STANDARD</c> or <c>EXPRESS</c>.</param>
/// <param name="CreationDate">The moment the state machine was created.</param>
public sealed record StateMachineSummary(
    string Name,
    string StateMachineArn,
    string Type,
    DateTimeOffset CreationDate);
