namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The desired request payload for testing an API Gateway REST API method invocation.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API.</param>
/// <param name="ResourceId">The identifier of the target resource.</param>
/// <param name="HttpMethod">The HTTP verb to invoke.</param>
/// <param name="PathWithQueryString">The resource path and optional query string used for invocation.</param>
/// <param name="Headers">The request headers to include.</param>
/// <param name="QueryStringParameters">The query string parameters to include.</param>
/// <param name="Body">The optional request body to send.</param>
/// <param name="StageVariables">The optional stage variables to apply during invocation.</param>
public sealed record RestMethodTestInvocationSpecification(
    string RestApiId,
    string ResourceId,
    string HttpMethod,
    string PathWithQueryString,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> QueryStringParameters,
    string? Body,
    IReadOnlyDictionary<string, string> StageVariables);
