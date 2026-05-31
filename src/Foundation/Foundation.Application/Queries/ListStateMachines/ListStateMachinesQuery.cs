using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.StepFunctions;

namespace Foundation.Application.Queries.ListStateMachines;

/// <summary>
/// List the Step Functions state machines available on the backend.
/// </summary>
public record ListStateMachinesQuery : IQuery<ListStateMachinesQueryResult>;

/// <summary>
/// The Step Functions state machines available on the backend.
/// </summary>
/// <param name="StateMachines">The state machines, ordered as returned by the backend.</param>
public record ListStateMachinesQueryResult(IReadOnlyList<StateMachineSummary> StateMachines);
