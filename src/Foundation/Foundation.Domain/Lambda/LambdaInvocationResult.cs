namespace Foundation.Domain.Lambda;

/// <summary>
/// The outcome of synchronously invoking a Lambda function (request/response).
/// </summary>
/// <param name="StatusCode">The HTTP status code returned by the Lambda service; 200 indicates the function ran.</param>
/// <param name="Payload">The response payload returned by the function, as a UTF-8 string; empty when none was returned.</param>
/// <param name="LogTail">The tail of the execution log, including the billed-duration report line; empty when none was returned.</param>
/// <param name="FunctionError">The error type reported by Lambda, for example <c>Unhandled</c>; empty when the function executed successfully.</param>
/// <param name="DurationMs">The wall-clock duration of the invocation in milliseconds.</param>
public sealed record LambdaInvocationResult(
    int StatusCode,
    string Payload,
    string LogTail,
    string FunctionError,
    long DurationMs)
{
    /// <summary>
    /// Gets a value indicating whether the function reported an error during execution.
    /// </summary>
    public bool HasFunctionError => !string.IsNullOrEmpty(FunctionError);
}
