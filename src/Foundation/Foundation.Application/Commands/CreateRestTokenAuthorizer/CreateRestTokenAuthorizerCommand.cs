using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateRestTokenAuthorizer;

/// <summary>
/// Create an OAuth/JWT token authorizer on an API Gateway REST API. The authorizer validates
/// bearer tokens issued by an OIDC provider through a custom authorizer function.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the authorizer belongs to.</param>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="Issuer">The OIDC issuer (<c>iss</c>) the tokens are expected to originate from.</param>
/// <param name="Audience">The audience (<c>aud</c>) the tokens are expected to be issued for.</param>
/// <param name="IdentitySource">The request location the bearer token is read from.</param>
/// <param name="AuthorizerUri">The invocation URI of the function that validates the bearer token.</param>
public record CreateRestTokenAuthorizerCommand(
    string RestApiId,
    string Name,
    string Issuer,
    string Audience,
    string IdentitySource,
    string AuthorizerUri) : ICommand<string>;
