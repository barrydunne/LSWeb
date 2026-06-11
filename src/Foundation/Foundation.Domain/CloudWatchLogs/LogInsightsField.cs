namespace Foundation.Domain.CloudWatchLogs;

/// <summary>
/// A single field within a CloudWatch Logs Insights result row.
/// </summary>
/// <param name="Field">The name of the field, for example <c>@timestamp</c> or <c>@message</c>.</param>
/// <param name="Value">The value of the field for the owning row.</param>
public sealed record LogInsightsField(
    string Field,
    string Value);
