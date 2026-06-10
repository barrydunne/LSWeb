namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The desired CORS policy for an API Gateway REST API resource.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the resource belongs to.</param>
/// <param name="ResourceId">The identifier of the resource to configure.</param>
/// <param name="AllowOrigins">The origins to allow (the <c>Access-Control-Allow-Origin</c> values).</param>
/// <param name="AllowMethods">The HTTP methods to allow (the <c>Access-Control-Allow-Methods</c> values).</param>
/// <param name="AllowHeaders">The request headers to allow (the <c>Access-Control-Allow-Headers</c> values).</param>
public sealed record RestCorsSpecification(
    string RestApiId,
    string ResourceId,
    IReadOnlyList<string> AllowOrigins,
    IReadOnlyList<string> AllowMethods,
    IReadOnlyList<string> AllowHeaders);
