namespace Foundation.Domain.Lambda;

/// <summary>
/// Derived monitoring information for a Lambda function, calculated from its recent CloudWatch log events.
/// </summary>
/// <param name="Metrics">The aggregate metrics derived from the recent invocations.</param>
/// <param name="RecentInvocations">The recent invocations, ordered newest first.</param>
public sealed record LambdaInvocationInsights(
    LambdaInvocationMetrics Metrics,
    IReadOnlyList<LambdaRecentInvocation> RecentInvocations);

/// <summary>
/// Aggregate metrics derived from a Lambda function's recent invocations.
/// </summary>
/// <param name="InvocationCount">The number of completed invocations observed.</param>
/// <param name="ErrorCount">The number of observed invocations that reported an error.</param>
/// <param name="AverageDurationMs">The mean execution duration in milliseconds; zero when no invocations were observed.</param>
/// <param name="MaxDurationMs">The longest execution duration in milliseconds; zero when no invocations were observed.</param>
public sealed record LambdaInvocationMetrics(
    int InvocationCount,
    int ErrorCount,
    double AverageDurationMs,
    double MaxDurationMs);

/// <summary>
/// A single completed Lambda invocation derived from its CloudWatch log events.
/// </summary>
/// <param name="RequestId">The Lambda request identifier for the invocation.</param>
/// <param name="Timestamp">The time the invocation completed, in ISO 8601 form; empty when not reported.</param>
/// <param name="DurationMs">The reported execution duration in milliseconds.</param>
/// <param name="HasError">Whether the invocation's log events reported an error.</param>
public sealed record LambdaRecentInvocation(
    string RequestId,
    string Timestamp,
    double DurationMs,
    bool HasError);
