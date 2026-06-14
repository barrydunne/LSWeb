using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteRoute53Record;

/// <summary>
/// Delete a DNS resource record set from a Route 53 hosted zone.
/// </summary>
/// <param name="HostedZoneId">The identifier of the hosted zone.</param>
/// <param name="Name">The fully qualified record name.</param>
/// <param name="Type">The DNS record type.</param>
/// <param name="Ttl">The time to live in seconds of the existing record.</param>
/// <param name="Values">The values of the existing record.</param>
public record DeleteRoute53RecordCommand(
    string HostedZoneId,
    string Name,
    string Type,
    long Ttl,
    IReadOnlyList<string> Values) : ICommand;
