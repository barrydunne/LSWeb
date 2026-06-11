namespace Foundation.Domain.CloudWatchLogs;

/// <summary>
/// The outcome of running a CloudWatch Logs Insights query, including its terminal status and the
/// rows the backend matched.
/// </summary>
/// <param name="Status">The terminal query status reported by the backend, for example <c>Complete</c>.</param>
/// <param name="Rows">The result rows in the order returned by the backend.</param>
/// <param name="RecordsMatched">The number of log records that matched the query.</param>
/// <param name="RecordsScanned">The number of log records the backend scanned to run the query.</param>
public sealed record LogInsightsResult(
    string Status,
    IReadOnlyList<LogInsightsRow> Rows,
    long RecordsMatched,
    long RecordsScanned);
