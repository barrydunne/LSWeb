using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.RefreshSearch;

/// <summary>
/// Command that requests an immediate rebuild of the global search index and reports its progress to
/// connected clients through operation-lifecycle notifications.
/// </summary>
public record RefreshSearchCommand : ICommand;
