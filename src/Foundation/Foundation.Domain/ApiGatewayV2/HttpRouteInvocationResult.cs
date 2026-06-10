namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// The result of invoking an Amazon API Gateway v2 route to verify its authorization behaviour.
/// </summary>
/// <param name="StatusCode">The HTTP status code returned by the route invocation.</param>
/// <param name="Authorized">Whether the request was authorized; <see langword="false"/> when the status code was 401 or 403.</param>
/// <param name="LatencyMilliseconds">The round-trip latency of the invocation in milliseconds.</param>
/// <param name="Headers">The response headers returned by the invocation.</param>
/// <param name="Body">The response body returned by the invocation.</param>
public sealed record HttpRouteInvocationResult(
    int StatusCode,
    bool Authorized,
    long LatencyMilliseconds,
    IReadOnlyDictionary<string, string> Headers,
    string Body);
