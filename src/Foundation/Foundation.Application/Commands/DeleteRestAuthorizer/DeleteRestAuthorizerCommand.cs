using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteRestAuthorizer;

/// <summary>
/// Delete an authorizer from an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the authorizer belongs to.</param>
/// <param name="AuthorizerId">The identifier of the authorizer to delete.</param>
public record DeleteRestAuthorizerCommand(string RestApiId, string AuthorizerId) : ICommand;
