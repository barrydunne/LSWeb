using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateHttpAuthorizer;

/// <summary>
/// Update an existing Amazon API Gateway v2 authorizer with the supplied configuration.
/// </summary>
/// <param name="ApiId">The identifier of the API the authorizer belongs to.</param>
/// <param name="AuthorizerId">The identifier of the authorizer to update.</param>
/// <param name="Name">The human-readable name of the authorizer.</param>
/// <param name="AuthorizerType">The type of the authorizer (for example <c>JWT</c>).</param>
/// <param name="IdentitySource">The identity sources the authorizer reads the token from (for example <c>$request.header.Authorization</c>).</param>
/// <param name="JwtIssuer">The OpenID issuer URL for a JWT authorizer, or <see langword="null"/> when not applicable.</param>
/// <param name="JwtAudience">The allowed audiences for a JWT authorizer (the Cognito app client identifiers).</param>
public record UpdateHttpAuthorizerCommand(
    string ApiId,
    string AuthorizerId,
    string Name,
    string AuthorizerType,
    IReadOnlyList<string> IdentitySource,
    string? JwtIssuer,
    IReadOnlyList<string> JwtAudience) : ICommand;
