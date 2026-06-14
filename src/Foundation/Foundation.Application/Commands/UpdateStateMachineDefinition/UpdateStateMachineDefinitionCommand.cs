using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateStateMachineDefinition;

/// <summary>
/// Update the Amazon States Language definition of an existing Step Functions state machine.
/// </summary>
/// <param name="StateMachineArn">The Amazon Resource Name of the state machine to update.</param>
/// <param name="Definition">The new Amazon States Language definition as a JSON document.</param>
public record UpdateStateMachineDefinitionCommand(string StateMachineArn, string Definition) : ICommand;
