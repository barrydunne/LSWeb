namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// A concise view of an Amazon API Gateway v2 authorizer as it appears in a list.
/// </summary>
/// <param name="AuthorizerId">The unique identifier of the authorizer.</param>
/// <param name="Name">The human-readable name of the authorizer.</param>
/// <param name="AuthorizerType">The type of the authorizer (for example <c>JWT</c> or <c>REQUEST</c>).</param>
public sealed record HttpAuthorizerSummary(
    string AuthorizerId,
    string Name,
    string AuthorizerType);
