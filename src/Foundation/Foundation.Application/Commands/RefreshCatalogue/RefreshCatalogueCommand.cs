using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.RefreshCatalogue;

/// <summary>
/// Command that refreshes the managed service catalogue and reports its progress to connected
/// clients through operation-lifecycle notifications.
/// </summary>
public record RefreshCatalogueCommand : ICommand;
