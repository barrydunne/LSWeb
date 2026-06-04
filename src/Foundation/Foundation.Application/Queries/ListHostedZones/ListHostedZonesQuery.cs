using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Route53;

namespace Foundation.Application.Queries.ListHostedZones;

/// <summary>
/// List the Route 53 hosted zones available on the backend.
/// </summary>
public record ListHostedZonesQuery : IQuery<ListHostedZonesQueryResult>;

/// <summary>
/// The Route 53 hosted zones available on the backend.
/// </summary>
/// <param name="HostedZones">The hosted zones, ordered as returned by the backend.</param>
public record ListHostedZonesQueryResult(IReadOnlyList<HostedZone> HostedZones);
