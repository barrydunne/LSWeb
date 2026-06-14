using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpsertRoute53Record;

/// <summary>
/// Create or replace a DNS resource record set in a Route 53 hosted zone.
/// </summary>
/// <param name="HostedZoneId">The identifier of the hosted zone.</param>
/// <param name="Name">The fully qualified record name.</param>
/// <param name="Type">The DNS record type, for example <c>A</c>, <c>CNAME</c>, <c>TXT</c> or <c>MX</c>.</param>
/// <param name="Ttl">The time to live in seconds.</param>
/// <param name="Values">The record values.</param>
public record UpsertRoute53RecordCommand(
    string HostedZoneId,
    string Name,
    string Type,
    long Ttl,
    IReadOnlyList<string> Values) : ICommand;
