using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetLiveness;

/// <summary>
/// Query that reports whether the service is alive and able to serve requests.
/// </summary>
public record GetLivenessQuery : IQuery<GetLivenessQueryResult>;

/// <summary>
/// The result of a <see cref="GetLivenessQuery"/>.
/// </summary>
/// <param name="Status">A short status indicator, for example <c>Healthy</c>.</param>
public record GetLivenessQueryResult(string Status);
