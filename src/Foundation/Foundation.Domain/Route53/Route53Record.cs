namespace Foundation.Domain.Route53;

/// <summary>
/// A DNS resource record set within a Route 53 hosted zone.
/// </summary>
/// <param name="Name">The fully qualified record name, for example <c>www.example.com.</c>.</param>
/// <param name="Type">The DNS record type, for example <c>A</c>, <c>CNAME</c>, <c>TXT</c> or <c>MX</c>.</param>
/// <param name="Ttl">The time to live in seconds.</param>
/// <param name="Values">The record values; multiple values for multi-valued record sets.</param>
public sealed record Route53Record(
    string Name,
    string Type,
    long Ttl,
    IReadOnlyList<string> Values);
