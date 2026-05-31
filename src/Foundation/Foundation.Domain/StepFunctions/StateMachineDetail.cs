namespace Foundation.Domain.StepFunctions;

/// <summary>
/// The full details of a Step Functions state machine.
/// </summary>
/// <param name="Name">The state machine name.</param>
/// <param name="StateMachineArn">The Amazon Resource Name that uniquely identifies the state machine.</param>
/// <param name="Type">The state machine type, either <c>STANDARD</c> or <c>EXPRESS</c>.</param>
/// <param name="Status">The state machine status, for example <c>ACTIVE</c> or <c>DELETING</c>.</param>
/// <param name="RoleArn">The Amazon Resource Name of the IAM role the state machine assumes.</param>
/// <param name="Definition">The Amazon States Language definition as a JSON document.</param>
/// <param name="CreationDate">The moment the state machine was created.</param>
public sealed record StateMachineDetail(
    string Name,
    string StateMachineArn,
    string Type,
    string Status,
    string RoleArn,
    string Definition,
    DateTimeOffset CreationDate);
