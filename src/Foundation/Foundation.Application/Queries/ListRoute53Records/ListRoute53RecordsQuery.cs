using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Route53;

namespace Foundation.Application.Queries.ListRoute53Records;

/// <summary>
/// List the DNS resource record sets in a Route 53 hosted zone.
/// </summary>
/// <param name="HostedZoneId">The identifier of the hosted zone.</param>
public record ListRoute53RecordsQuery(string HostedZoneId) : IQuery<ListRoute53RecordsQueryResult>;

/// <summary>
/// The DNS records in the requested hosted zone.
/// </summary>
/// <param name="Records">The resource record sets.</param>
public record ListRoute53RecordsQueryResult(IReadOnlyList<Route53Record> Records);
