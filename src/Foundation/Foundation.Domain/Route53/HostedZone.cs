namespace Foundation.Domain.Route53;

/// <summary>
/// A concise view of a Route 53 hosted zone as it appears in a list.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the hosted zone.</param>
/// <param name="Name">The domain name of the hosted zone.</param>
/// <param name="RecordCount">The number of resource record sets in the zone.</param>
/// <param name="PrivateZone">Whether the hosted zone is a private zone associated with a VPC.</param>
public sealed record HostedZone(
    string Id,
    string Name,
    long RecordCount,
    bool PrivateZone);
