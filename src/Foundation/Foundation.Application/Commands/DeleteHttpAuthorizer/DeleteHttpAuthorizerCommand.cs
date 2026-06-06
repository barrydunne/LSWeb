using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteHttpAuthorizer;

/// <summary>
/// Delete an Amazon API Gateway v2 authorizer. This action cannot be undone.
/// </summary>
/// <param name="ApiId">The identifier of the API the authorizer belongs to.</param>
/// <param name="AuthorizerId">The unique identifier of the authorizer to delete.</param>
public record DeleteHttpAuthorizerCommand(string ApiId, string AuthorizerId) : ICommand;
