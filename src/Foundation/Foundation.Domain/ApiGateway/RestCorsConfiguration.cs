namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The CORS policy currently configured on an API Gateway REST API resource.
/// </summary>
/// <param name="ResourceId">The identifier of the resource the policy applies to.</param>
/// <param name="Enabled">Whether a CORS preflight (OPTIONS) policy is configured on the resource.</param>
/// <param name="AllowOrigins">The origins the policy allows (the <c>Access-Control-Allow-Origin</c> values).</param>
/// <param name="AllowMethods">The HTTP methods the policy allows (the <c>Access-Control-Allow-Methods</c> values).</param>
/// <param name="AllowHeaders">The request headers the policy allows (the <c>Access-Control-Allow-Headers</c> values).</param>
public sealed record RestCorsConfiguration(
    string ResourceId,
    bool Enabled,
    IReadOnlyList<string> AllowOrigins,
    IReadOnlyList<string> AllowMethods,
    IReadOnlyList<string> AllowHeaders);
