using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteStateMachine;

/// <summary>
/// Delete a Step Functions state machine by its Amazon Resource Name. This is a destructive action
/// that cannot be undone.
/// </summary>
/// <param name="StateMachineArn">The Amazon Resource Name of the state machine to delete.</param>
public record DeleteStateMachineCommand(string StateMachineArn) : ICommand;
