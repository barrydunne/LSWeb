using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.RemoveFavourite;

/// <summary>
/// Command that unpins a resource from the favourites list and persists the change.
/// </summary>
/// <param name="Reference">The resource reference (ARN or identifier) to unpin.</param>
public record RemoveFavouriteCommand(string Reference) : ICommand;
