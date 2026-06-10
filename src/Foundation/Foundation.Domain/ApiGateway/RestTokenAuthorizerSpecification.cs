namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The desired configuration of an API Gateway REST API token authorizer that validates
/// OAuth/JWT bearer tokens by way of a custom authorizer function.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the authorizer belongs to.</param>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="AuthorizerUri">The invocation URI of the function that validates the bearer token.</param>
/// <param name="IdentitySource">The request location the bearer token is read from.</param>
public sealed record RestTokenAuthorizerSpecification(
    string RestApiId,
    string Name,
    string AuthorizerUri,
    string IdentitySource);
