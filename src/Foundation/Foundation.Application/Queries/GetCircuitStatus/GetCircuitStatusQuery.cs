using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetCircuitStatus;

/// <summary>
/// Query the current AWS gateway circuit-breaker status.
/// </summary>
public record GetCircuitStatusQuery : IQuery<GetCircuitStatusQueryResult>;

/// <summary>
/// The result of a circuit-breaker status query.
/// </summary>
/// <param name="IsOpen">Whether at least one service has an open circuit breaker.</param>
/// <param name="AffectedServices">The catalogue keys of services whose calls are currently being rejected.</param>
public record GetCircuitStatusQueryResult(bool IsOpen, IReadOnlyList<string> AffectedServices);
