using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.AddFavourite;

/// <summary>
/// Command that pins a resource as a favourite and persists the change.
/// </summary>
/// <param name="Reference">The resource reference (ARN or identifier) to pin.</param>
public record AddFavouriteCommand(string Reference) : ICommand;
