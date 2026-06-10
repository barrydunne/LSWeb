using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Commands.TestInvokeRestMethod;

/// <summary>
/// Test invoke an API Gateway REST API method.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API.</param>
/// <param name="ResourceId">The identifier of the resource.</param>
/// <param name="HttpMethod">The HTTP verb to invoke.</param>
/// <param name="PathWithQueryString">The path and query string to invoke.</param>
/// <param name="Headers">The request headers to include.</param>
/// <param name="QueryStringParameters">The query string parameters to include.</param>
/// <param name="Body">The optional request body.</param>
/// <param name="StageVariables">The optional stage variables for invocation.</param>
public record TestInvokeRestMethodCommand(
    string RestApiId,
    string ResourceId,
    string HttpMethod,
    string PathWithQueryString,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> QueryStringParameters,
    string? Body,
    IReadOnlyDictionary<string, string> StageVariables)
    : ICommand<RestMethodTestInvocationResult>;
