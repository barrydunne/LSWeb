namespace Foundation.Domain.ApiGateway;

/// <summary>
/// A concise view of an API Gateway REST API authorizer as it appears in a list.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the authorizer.</param>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="Type">The authorizer type (for example <c>COGNITO_USER_POOLS</c>).</param>
public sealed record RestAuthorizerSummary(
    string Id,
    string Name,
    string Type);
