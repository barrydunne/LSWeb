namespace Foundation.Domain.CloudWatchLogs;

/// <summary>
/// A single row returned by a CloudWatch Logs Insights query, made up of the fields the query
/// selected.
/// </summary>
/// <param name="Fields">The fields that make up the row, in the order returned by the backend.</param>
public sealed record LogInsightsRow(
    IReadOnlyList<LogInsightsField> Fields);
