using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateHttpAuthorizer;

/// <summary>
/// Create an Amazon API Gateway v2 authorizer from the supplied configuration.
/// </summary>
/// <param name="ApiId">The identifier of the API the authorizer belongs to.</param>
/// <param name="Name">The human-readable name of the authorizer.</param>
/// <param name="AuthorizerType">The type of the authorizer (for example <c>JWT</c>).</param>
/// <param name="IdentitySource">The identity sources the authorizer reads the token from (for example <c>$request.header.Authorization</c>).</param>
/// <param name="JwtIssuer">The OpenID issuer URL for a JWT authorizer, or <see langword="null"/> when not applicable.</param>
/// <param name="JwtAudience">The allowed audiences for a JWT authorizer (the Cognito app client identifiers).</param>
public record CreateHttpAuthorizerCommand(
    string ApiId,
    string Name,
    string AuthorizerType,
    IReadOnlyList<string> IdentitySource,
    string? JwtIssuer,
    IReadOnlyList<string> JwtAudience) : ICommand<string>;
