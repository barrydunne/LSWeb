using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateRestAuthorizer;

/// <summary>
/// Update an existing Cognito user pool authorizer on an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the authorizer belongs to.</param>
/// <param name="AuthorizerId">The identifier of the authorizer to update.</param>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="Type">The authorizer type. Only <c>COGNITO_USER_POOLS</c> is supported.</param>
/// <param name="ProviderARNs">The Cognito user pool ARNs the authorizer trusts.</param>
/// <param name="IdentitySource">The request location the identity token is read from, or <c>null</c> to use the default.</param>
public record UpdateRestAuthorizerCommand(
    string RestApiId,
    string AuthorizerId,
    string Name,
    string Type,
    IReadOnlyList<string> ProviderARNs,
    string? IdentitySource) : ICommand;
