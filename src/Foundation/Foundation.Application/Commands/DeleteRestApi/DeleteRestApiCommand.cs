using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteRestApi;

/// <summary>
/// Delete an API Gateway REST API. This action cannot be undone.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API to delete.</param>
public record DeleteRestApiCommand(string RestApiId) : ICommand;
