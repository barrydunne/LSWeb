namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The result of testing an API Gateway REST API method invocation.
/// </summary>
/// <param name="StatusCode">The returned HTTP status code.</param>
/// <param name="LatencyMilliseconds">The invocation latency in milliseconds.</param>
/// <param name="Headers">The returned response headers.</param>
/// <param name="Body">The returned response body.</param>
/// <param name="Log">The execution log output returned by API Gateway.</param>
public sealed record RestMethodTestInvocationResult(
    int StatusCode,
    int LatencyMilliseconds,
    IReadOnlyDictionary<string, string> Headers,
    string Body,
    string? Log);
