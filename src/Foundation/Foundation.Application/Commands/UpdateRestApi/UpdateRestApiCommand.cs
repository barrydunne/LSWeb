using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateRestApi;

/// <summary>
/// Update the configuration of an existing API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API to update.</param>
/// <param name="Name">The name of the REST API.</param>
/// <param name="Description">The description of the REST API, or <see langword="null"/> for none.</param>
public record UpdateRestApiCommand(
    string RestApiId,
    string Name,
    string? Description) : ICommand;
