namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// The full configuration of an Amazon API Gateway v2 authorizer.
/// </summary>
/// <param name="AuthorizerId">The unique identifier of the authorizer.</param>
/// <param name="Name">The human-readable name of the authorizer.</param>
/// <param name="AuthorizerType">The type of the authorizer (for example <c>JWT</c> or <c>REQUEST</c>).</param>
/// <param name="IdentitySource">The identity sources the authorizer reads the token from (for example <c>$request.header.Authorization</c>).</param>
/// <param name="JwtIssuer">The OpenID issuer URL for a JWT authorizer, or <see langword="null"/> when not applicable.</param>
/// <param name="JwtAudience">The allowed audiences for a JWT authorizer (the Cognito app client identifiers).</param>
public sealed record HttpAuthorizerDetail(
    string AuthorizerId,
    string Name,
    string AuthorizerType,
    IReadOnlyList<string> IdentitySource,
    string? JwtIssuer,
    IReadOnlyList<string> JwtAudience);
