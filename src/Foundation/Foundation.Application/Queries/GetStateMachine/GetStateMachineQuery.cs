using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.StepFunctions;

namespace Foundation.Application.Queries.GetStateMachine;

/// <summary>
/// Get the details of a single Step Functions state machine.
/// </summary>
/// <param name="StateMachineArn">The Amazon Resource Name of the state machine to describe.</param>
public record GetStateMachineQuery(string StateMachineArn) : IQuery<GetStateMachineQueryResult>;

/// <summary>
/// The details of a single Step Functions state machine.
/// </summary>
/// <param name="StateMachine">The state machine details.</param>
public record GetStateMachineQueryResult(StateMachineDetail StateMachine);
