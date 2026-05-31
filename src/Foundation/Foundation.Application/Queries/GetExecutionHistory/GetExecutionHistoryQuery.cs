using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.StepFunctions;

namespace Foundation.Application.Queries.GetExecutionHistory;

/// <summary>
/// Get the ordered history of a single Step Functions execution.
/// </summary>
/// <param name="ExecutionArn">The Amazon Resource Name of the execution whose history to read.</param>
public record GetExecutionHistoryQuery(string ExecutionArn) : IQuery<GetExecutionHistoryQueryResult>;

/// <summary>
/// The ordered history of a single Step Functions execution.
/// </summary>
/// <param name="Events">The history events, ordered as returned by the backend.</param>
public record GetExecutionHistoryQueryResult(IReadOnlyList<ExecutionHistoryEvent> Events);
