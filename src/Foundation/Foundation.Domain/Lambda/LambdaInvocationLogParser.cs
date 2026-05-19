using System.Globalization;

namespace Foundation.Domain.Lambda;

/// <summary>
/// Derives invocation monitoring information from a Lambda function's raw CloudWatch log events.
/// Each completed invocation is identified by its <c>REPORT</c> line; metrics are aggregated across
/// the completed invocations. This is a best-effort derivation that degrades gracefully when the
/// expected markers are absent.
/// </summary>
public static class LambdaInvocationLogParser
{
    private const string RequestIdMarker = "RequestId: ";
    private const string DurationMarker = "Duration: ";

    private static readonly string[] _errorMarkers =
    [
        "[ERROR]",
        "Task timed out",
        "Unhandled",
        "errorMessage",
    ];

    /// <summary>
    /// Derives invocation insights from the supplied log events.
    /// </summary>
    /// <param name="logEvents">The raw log events to analyse.</param>
    /// <returns>The derived metrics and recent invocations.</returns>
    public static LambdaInvocationInsights Parse(IReadOnlyList<LambdaLogEvent> logEvents)
    {
        var invocations = new Dictionary<string, Accumulator>(StringComparer.Ordinal);
        var order = new List<string>();

        foreach (var logEvent in logEvents)
        {
            var requestId = ExtractRequestId(logEvent.Message);
            if (requestId is null)
            {
                continue;
            }

            if (!invocations.TryGetValue(requestId, out var accumulator))
            {
                accumulator = new Accumulator(requestId);
                invocations[requestId] = accumulator;
                order.Add(requestId);
            }

            if (ContainsErrorMarker(logEvent.Message))
            {
                accumulator.HasError = true;
            }

            var duration = ExtractDurationMs(logEvent.Message);
            if (duration is not null)
            {
                accumulator.DurationMs = duration.Value;
                accumulator.Timestamp = logEvent.Timestamp;
                accumulator.Completed = true;
            }
        }

        var recent = order
            .Select(requestId => invocations[requestId])
            .Where(accumulator => accumulator.Completed)
            .OrderByDescending(accumulator => accumulator.Timestamp, StringComparer.Ordinal)
            .Select(accumulator => new LambdaRecentInvocation(
                accumulator.RequestId,
                accumulator.Timestamp,
                accumulator.DurationMs,
                accumulator.HasError))
            .ToList();

        return new LambdaInvocationInsights(BuildMetrics(recent), recent);
    }

    private static LambdaInvocationMetrics BuildMetrics(List<LambdaRecentInvocation> invocations)
    {
        if (invocations.Count == 0)
        {
            return new LambdaInvocationMetrics(0, 0, 0, 0);
        }

        var errorCount = invocations.Count(invocation => invocation.HasError);
        var average = invocations.Average(invocation => invocation.DurationMs);
        var max = invocations.Max(invocation => invocation.DurationMs);
        return new LambdaInvocationMetrics(invocations.Count, errorCount, average, max);
    }

    private static string? ExtractRequestId(string message)
    {
        var index = message.IndexOf(RequestIdMarker, StringComparison.Ordinal);
        if (index < 0)
        {
            return null;
        }

        var start = index + RequestIdMarker.Length;
        var end = start;
        while (end < message.Length && !char.IsWhiteSpace(message[end]))
        {
            end++;
        }

        return end > start ? message[start..end] : null;
    }

    private static double? ExtractDurationMs(string message)
    {
        var index = message.IndexOf(DurationMarker, StringComparison.Ordinal);
        if (index < 0)
        {
            return null;
        }

        var start = index + DurationMarker.Length;
        var end = start;
        while (end < message.Length && message[end] != ' ')
        {
            end++;
        }

        return double.TryParse(message[start..end], NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static bool ContainsErrorMarker(string message)
    {
        foreach (var marker in _errorMarkers)
        {
            if (message.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private sealed class Accumulator
    {
        public Accumulator(string requestId) => RequestId = requestId;

        public string RequestId { get; }

        public string Timestamp { get; set; } = string.Empty;

        public double DurationMs { get; set; }

        public bool HasError { get; set; }

        public bool Completed { get; set; }
    }
}
