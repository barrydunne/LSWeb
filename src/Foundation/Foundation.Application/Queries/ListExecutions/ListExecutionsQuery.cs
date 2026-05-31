using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.StepFunctions;

namespace Foundation.Application.Queries.ListExecutions;

/// <summary>
/// List the executions of a single Step Functions state machine.
/// </summary>
/// <param name="StateMachineArn">The Amazon Resource Name of the state machine whose executions to list.</param>
public record ListExecutionsQuery(string StateMachineArn) : IQuery<ListExecutionsQueryResult>;

/// <summary>
/// The executions of a single Step Functions state machine.
/// </summary>
/// <param name="Executions">The executions, ordered as returned by the backend.</param>
public record ListExecutionsQueryResult(IReadOnlyList<ExecutionSummary> Executions);
