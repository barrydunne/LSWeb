using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.StepFunctions;

namespace Foundation.Application.Commands.StartExecution;

/// <summary>
/// Start a new execution of a Step Functions state machine, optionally with a name and JSON input.
/// </summary>
/// <param name="StateMachineArn">The Amazon Resource Name of the state machine to execute.</param>
/// <param name="Name">An optional name for the execution; the backend generates one when omitted.</param>
/// <param name="Input">An optional JSON document passed as the execution input.</param>
public record StartExecutionCommand(string StateMachineArn, string? Name, string? Input)
    : ICommand<ExecutionStartResult>;
