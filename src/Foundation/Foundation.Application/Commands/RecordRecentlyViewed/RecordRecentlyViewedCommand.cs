using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.RecordRecentlyViewed;

/// <summary>
/// Command that records that the user opened a resource, adding it to the front of the
/// recently-viewed list and persisting the change.
/// </summary>
/// <param name="Reference">The resource reference (ARN or identifier) that was opened.</param>
public record RecordRecentlyViewedCommand(string Reference) : ICommand;
