using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Queries.RequestCognitoToken;

/// <summary>
/// Requests bearer tokens for an Amazon Cognito app client and decodes the identity token claims.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
/// <param name="ClientId">The identifier of the app client to authenticate against.</param>
/// <param name="Username">The username to authenticate.</param>
/// <param name="Password">The password to authenticate with.</param>
public record RequestCognitoTokenQuery(
    string UserPoolId,
    string ClientId,
    string Username,
    string Password) : IQuery<RequestCognitoTokenQueryResult>;

/// <summary>
/// The result of a token request.
/// </summary>
/// <param name="Token">The issued tokens and decoded claims.</param>
public record RequestCognitoTokenQueryResult(TokenResult Token);
