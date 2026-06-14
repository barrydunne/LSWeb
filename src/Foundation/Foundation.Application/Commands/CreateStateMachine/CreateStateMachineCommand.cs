using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.StepFunctions;

namespace Foundation.Application.Commands.CreateStateMachine;

/// <summary>
/// Create a new Step Functions state machine from an Amazon States Language definition.
/// </summary>
/// <param name="Name">The name of the state machine.</param>
/// <param name="Definition">The Amazon States Language definition as a JSON document.</param>
/// <param name="RoleArn">The Amazon Resource Name of the IAM role the state machine assumes.</param>
/// <param name="Type">The state machine type, either <c>STANDARD</c> or <c>EXPRESS</c>.</param>
public record CreateStateMachineCommand(string Name, string Definition, string RoleArn, string Type)
    : ICommand<StateMachineCreateResult>;
