using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateStateMachine;
using Foundation.Application.Commands.StartExecution;
using Foundation.Application.Commands.UpdateStateMachineDefinition;
using Foundation.Application.Queries.GetExecutionHistory;
using Foundation.Application.Queries.GetStateMachine;
using Foundation.Application.Queries.ListExecutions;
using Foundation.Application.Queries.ListStateMachines;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS Step Functions: listing the available state machines and viewing the
/// details of a single state machine.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/step-functions")]
public partial class StepFunctionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StepFunctionsController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public StepFunctionsController(ISender sender, ILogger<StepFunctionsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the Step Functions state machines available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the state machine summaries.</returns>
    [HttpGet("state-machines")]
    [ProducesResponseType(typeof(StateMachineListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListStateMachines(CancellationToken cancellationToken)
    {
        LogHandlingListStateMachines();
        var result = await _sender.Send(new ListStateMachinesQuery(), cancellationToken);
        LogListStateMachinesHandled(result.IsSuccess);
        return result.Match(
            stateMachines => Results.Ok(new StateMachineListResponse(
                stateMachines.StateMachines
                    .Select(stateMachine => new StateMachineSummaryResponse(
                        stateMachine.Name,
                        stateMachine.StateMachineArn,
                        stateMachine.Type,
                        stateMachine.CreationDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single Step Functions state machine by its Amazon Resource Name.
    /// </summary>
    /// <param name="arn">The Amazon Resource Name of the state machine to describe.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the state machine details.</returns>
    [HttpGet("state-machine")]
    [ProducesResponseType(typeof(StateMachineDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetStateMachine(
        [FromQuery] string arn, CancellationToken cancellationToken)
    {
        LogHandlingGetStateMachine(arn);
        var result = await _sender.Send(new GetStateMachineQuery(arn), cancellationToken);
        LogGetStateMachineHandled(result.IsSuccess);
        return result.Match(
            stateMachine => Results.Ok(new StateMachineDetailResponse(
                stateMachine.StateMachine.Name,
                stateMachine.StateMachine.StateMachineArn,
                stateMachine.StateMachine.Type,
                stateMachine.StateMachine.Status,
                stateMachine.StateMachine.RoleArn,
                stateMachine.StateMachine.Definition,
                stateMachine.StateMachine.CreationDate)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the executions of a single Step Functions state machine.
    /// </summary>
    /// <param name="arn">The Amazon Resource Name of the state machine whose executions to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the execution summaries.</returns>
    [HttpGet("executions")]
    [ProducesResponseType(typeof(ExecutionListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListExecutions(
        [FromQuery] string arn, CancellationToken cancellationToken)
    {
        LogHandlingListExecutions(arn);
        var result = await _sender.Send(new ListExecutionsQuery(arn), cancellationToken);
        LogListExecutionsHandled(result.IsSuccess);
        return result.Match(
            executions => Results.Ok(new ExecutionListResponse(
                executions.Executions
                    .Select(execution => new ExecutionSummaryResponse(
                        execution.ExecutionArn,
                        execution.Name,
                        execution.StateMachineArn,
                        execution.Status,
                        execution.StartDate,
                        execution.StopDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Starts a new execution of a Step Functions state machine.
    /// </summary>
    /// <param name="request">The state machine ARN with an optional name and JSON input.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the started execution.</returns>
    [HttpPost("executions")]
    [ProducesResponseType(typeof(StartExecutionResponse), StatusCodes.Status200OK)]
    public async Task<IResult> StartExecution(
        [FromBody] StartExecutionRequest request, CancellationToken cancellationToken)
    {
        LogHandlingStartExecution(request.StateMachineArn);
        var result = await _sender.Send(
            new StartExecutionCommand(request.StateMachineArn, request.Name, request.Input),
            cancellationToken);
        LogStartExecutionHandled(result.IsSuccess);
        return result.Match(
            execution => Results.Ok(new StartExecutionResponse(
                execution.ExecutionArn,
                execution.StartDate)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new Step Functions state machine from an Amazon States Language definition.
    /// </summary>
    /// <param name="request">The state machine name, ASL definition, role ARN, and type.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the created state machine.</returns>
    [HttpPost("state-machines")]
    [ProducesResponseType(typeof(CreateStateMachineResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateStateMachine(
        [FromBody] CreateStateMachineRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateStateMachine(request.Name);
        var result = await _sender.Send(
            new CreateStateMachineCommand(request.Name, request.Definition, request.RoleArn, request.Type),
            cancellationToken);
        LogCreateStateMachineHandled(result.IsSuccess);
        return result.Match(
            stateMachine => Results.Created(
                $"/api/services/step-functions/state-machine?arn={Uri.EscapeDataString(stateMachine.StateMachineArn)}",
                new CreateStateMachineResponse(stateMachine.StateMachineArn, stateMachine.CreationDate)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates the Amazon States Language definition of an existing state machine.
    /// </summary>
    /// <param name="request">The state machine ARN and the new ASL definition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("state-machine/definition")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateStateMachineDefinition(
        [FromBody] UpdateStateMachineDefinitionRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateDefinition(request.StateMachineArn);
        var result = await _sender.Send(
            new UpdateStateMachineDefinitionCommand(request.StateMachineArn, request.Definition),
            cancellationToken);
        LogUpdateDefinitionHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the ordered history of a single Step Functions execution.
    /// </summary>
    /// <param name="arn">The Amazon Resource Name of the execution whose history to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the execution history events.</returns>
    [HttpGet("execution-history")]
    [ProducesResponseType(typeof(ExecutionHistoryResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetExecutionHistory(
        [FromQuery] string arn, CancellationToken cancellationToken)
    {
        LogHandlingGetExecutionHistory(arn);
        var result = await _sender.Send(new GetExecutionHistoryQuery(arn), cancellationToken);
        LogGetExecutionHistoryHandled(result.IsSuccess);
        return result.Match(
            history => Results.Ok(new ExecutionHistoryResponse(
                history.Events
                    .Select(historyEvent => new ExecutionHistoryEventResponse(
                        historyEvent.Id,
                        historyEvent.PreviousEventId,
                        historyEvent.Type,
                        historyEvent.Timestamp,
                        historyEvent.Name,
                        historyEvent.Input,
                        historyEvent.Output,
                        historyEvent.Error,
                        historyEvent.Cause))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling Step Functions state machine list request.")]
    private partial void LogHandlingListStateMachines();

    [LoggerMessage(LogLevel.Trace, "Step Functions state machine list request handled. Success: {Success}")]
    private partial void LogListStateMachinesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Step Functions state machine get request for {Arn}.")]
    private partial void LogHandlingGetStateMachine(string arn);

    [LoggerMessage(LogLevel.Trace, "Step Functions state machine get request handled. Success: {Success}")]
    private partial void LogGetStateMachineHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Step Functions execution list request for {Arn}.")]
    private partial void LogHandlingListExecutions(string arn);

    [LoggerMessage(LogLevel.Trace, "Step Functions execution list request handled. Success: {Success}")]
    private partial void LogListExecutionsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Step Functions start execution request for {Arn}.")]
    private partial void LogHandlingStartExecution(string arn);

    [LoggerMessage(LogLevel.Trace, "Step Functions start execution request handled. Success: {Success}")]
    private partial void LogStartExecutionHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Step Functions create state machine request for {Name}.")]
    private partial void LogHandlingCreateStateMachine(string name);

    [LoggerMessage(LogLevel.Trace, "Step Functions create state machine request handled. Success: {Success}")]
    private partial void LogCreateStateMachineHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Step Functions update definition request for {Arn}.")]
    private partial void LogHandlingUpdateDefinition(string arn);

    [LoggerMessage(LogLevel.Trace, "Step Functions update definition request handled. Success: {Success}")]
    private partial void LogUpdateDefinitionHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Step Functions execution history request for {Arn}.")]
    private partial void LogHandlingGetExecutionHistory(string arn);

    [LoggerMessage(LogLevel.Trace, "Step Functions execution history request handled. Success: {Success}")]
    private partial void LogGetExecutionHistoryHandled(bool success);
}
