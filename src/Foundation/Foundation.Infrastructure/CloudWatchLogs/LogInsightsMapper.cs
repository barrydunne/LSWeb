using Amazon.CloudWatchLogs.Model;
using Foundation.Domain.CloudWatchLogs;

namespace Foundation.Infrastructure.CloudWatchLogs;

/// <summary>
/// Translates AWS CloudWatch Logs Insights query responses into the domain records the application
/// works with, applying safe defaults for missing values.
/// </summary>
internal static class LogInsightsMapper
{
    /// <summary>
    /// Map an SDK Insights query response to the domain result.
    /// </summary>
    /// <param name="status">The terminal query status reported by the backend.</param>
    /// <param name="response">The query results response returned by the backend.</param>
    /// <returns>The domain Insights result.</returns>
    public static LogInsightsResult ToResult(string status, GetQueryResultsResponse response)
    {
        IReadOnlyList<LogInsightsRow> rows = (response.Results ?? [])
            .Select(ToRow)
            .ToList();

        var matched = ToCount(response.Statistics?.RecordsMatched);
        var scanned = ToCount(response.Statistics?.RecordsScanned);

        return new LogInsightsResult(status, rows, matched, scanned);
    }

    private static LogInsightsRow ToRow(List<ResultField> fields)
    {
        IReadOnlyList<LogInsightsField> mapped = (fields ?? [])
            .Select(field => new LogInsightsField(field.Field ?? string.Empty, field.Value ?? string.Empty))
            .ToList();
        return new LogInsightsRow(mapped);
    }

    private static long ToCount(double? value)
        => value is null
            ? 0
            : (long)Math.Round(value.Value, MidpointRounding.AwayFromZero);
}
