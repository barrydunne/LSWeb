namespace Foundation.Domain.Lambda;

/// <summary>
/// The outcome of issuing a test HTTP request against a Lambda function URL.
/// </summary>
/// <param name="StatusCode">The HTTP status code returned by the function URL.</param>
/// <param name="Body">The response body returned by the function URL, truncated for display.</param>
public sealed record LambdaFunctionUrlTest(
    int StatusCode,
    string Body);
