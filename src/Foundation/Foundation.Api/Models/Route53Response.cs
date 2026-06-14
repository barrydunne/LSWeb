namespace Foundation.Api.Models;

/// <summary>
/// The Route 53 hosted zones available on the backend.
/// </summary>
/// <param name="HostedZones">The hosted zone summaries, ordered as returned by the backend.</param>
public sealed record HostedZoneListResponse(
    IReadOnlyList<HostedZoneSummaryResponse> HostedZones);

/// <summary>
/// A concise view of a Route 53 hosted zone as it appears in a list.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the hosted zone.</param>
/// <param name="Name">The domain name of the hosted zone.</param>
/// <param name="RecordCount">The number of resource record sets in the zone.</param>
/// <param name="PrivateZone">Whether the hosted zone is a private zone associated with a VPC.</param>
public sealed record HostedZoneSummaryResponse(
    string Id,
    string Name,
    long RecordCount,
    bool PrivateZone);

/// <summary>
/// The request to create a Route 53 public hosted zone.
/// </summary>
/// <param name="Name">The fully qualified domain name for the hosted zone.</param>
/// <param name="Comment">An optional comment describing the zone.</param>
public sealed record HostedZoneCreateRequest(
    string Name,
    string? Comment);

/// <summary>
/// The DNS resource record sets in a Route 53 hosted zone.
/// </summary>
/// <param name="Records">The resource record sets.</param>
public sealed record Route53RecordListResponse(
    IReadOnlyList<Route53RecordResponse> Records);

/// <summary>
/// A DNS resource record set within a Route 53 hosted zone.
/// </summary>
/// <param name="Name">The fully qualified record name.</param>
/// <param name="Type">The DNS record type.</param>
/// <param name="Ttl">The time to live in seconds.</param>
/// <param name="Values">The record values.</param>
public sealed record Route53RecordResponse(
    string Name,
    string Type,
    long Ttl,
    IReadOnlyList<string> Values);

/// <summary>
/// The request to create, replace or delete a DNS resource record set.
/// </summary>
/// <param name="Name">The fully qualified record name.</param>
/// <param name="Type">The DNS record type.</param>
/// <param name="Ttl">The time to live in seconds.</param>
/// <param name="Values">The record values.</param>
public sealed record Route53RecordRequest(
    string Name,
    string Type,
    long Ttl,
    IReadOnlyList<string> Values);
