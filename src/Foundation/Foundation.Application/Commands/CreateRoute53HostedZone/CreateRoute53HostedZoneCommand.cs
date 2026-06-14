using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateRoute53HostedZone;

/// <summary>
/// Create a Route 53 public hosted zone.
/// </summary>
/// <param name="Name">The fully qualified domain name for the hosted zone.</param>
/// <param name="Comment">An optional comment describing the zone.</param>
public record CreateRoute53HostedZoneCommand(string Name, string? Comment) : ICommand;
