using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Health;

namespace Foundation.Application.Queries.GetHealth;

/// <summary>
/// Query the latest backend health snapshot for the managed AWS services.
/// </summary>
public record GetHealthQuery : IQuery<GetHealthQueryResult>;

/// <summary>
/// The result of a health query.
/// </summary>
/// <param name="Services">The per-service availability results in catalogue order.</param>
public record GetHealthQueryResult(IReadOnlyList<ServiceHealth> Services);
