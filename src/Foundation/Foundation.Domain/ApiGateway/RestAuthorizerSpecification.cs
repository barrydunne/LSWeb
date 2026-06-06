namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The desired configuration of an API Gateway REST API authorizer backed by a Cognito user pool.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the authorizer belongs to.</param>
/// <param name="AuthorizerId">The identifier of the authorizer to update, or <see langword="null"/> when creating.</param>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="Type">The authorizer type. Only <c>COGNITO_USER_POOLS</c> is supported.</param>
/// <param name="ProviderARNs">The Cognito user pool ARNs the authorizer trusts.</param>
/// <param name="IdentitySource">The request location the identity token is read from, or <see langword="null"/> to use the default.</param>
public sealed record RestAuthorizerSpecification(
    string RestApiId,
    string? AuthorizerId,
    string Name,
    string Type,
    IReadOnlyList<string> ProviderARNs,
    string? IdentitySource);
