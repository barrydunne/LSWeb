using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.StepFunctions;

namespace Foundation.Application.StepFunctions;

/// <summary>
/// Abstracts the Step Functions operations the application needs so the handlers stay free of any
/// direct AWS SDK dependency. The implementation flows every call through the resilient AWS gateway
/// and translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface IStepFunctionsClient
{
    /// <summary>
    /// List the state machines available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The state machines, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<StateMachineSummary>>> ListStateMachinesAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Describe a single state machine by its Amazon Resource Name.
    /// </summary>
    /// <param name="stateMachineArn">The Amazon Resource Name of the state machine to describe.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The state machine details, or an error when the backend cannot be reached.</returns>
    Task<Result<StateMachineDetail>> DescribeStateMachineAsync(
        string stateMachineArn, CancellationToken cancellationToken);

    /// <summary>
    /// List the executions of a single state machine, most recent first.
    /// </summary>
    /// <param name="stateMachineArn">The Amazon Resource Name of the state machine whose executions to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The executions, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<ExecutionSummary>>> ListExecutionsAsync(
        string stateMachineArn, CancellationToken cancellationToken);

    /// <summary>
    /// Start a new execution of a state machine, optionally with a name and JSON input.
    /// </summary>
    /// <param name="stateMachineArn">The Amazon Resource Name of the state machine to execute.</param>
    /// <param name="name">An optional name for the execution; the backend generates one when omitted.</param>
    /// <param name="input">An optional JSON document passed as the execution input.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The started execution, or an error when the backend cannot be reached.</returns>
    Task<Result<ExecutionStartResult>> StartExecutionAsync(
        string stateMachineArn, string? name, string? input, CancellationToken cancellationToken);

    /// <summary>
    /// Get the ordered history of a single execution, including per-state input and output.
    /// </summary>
    /// <param name="executionArn">The Amazon Resource Name of the execution whose history to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The history events in order, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<ExecutionHistoryEvent>>> GetExecutionHistoryAsync(
        string executionArn, CancellationToken cancellationToken);
}
