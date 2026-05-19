using System.Globalization;
using Amazon.CloudWatchLogs.Model;
using Amazon.Lambda.Model;
using Foundation.Domain.Lambda;

namespace Foundation.Infrastructure.Lambda;

/// <summary>
/// Translates AWS Lambda SDK models into the domain records the application works with, applying
/// safe defaults for any field the backend leaves unset.
/// </summary>
internal static class LambdaFunctionMapper
{
    /// <summary>
    /// Map an AWS function configuration to a list summary.
    /// </summary>
    /// <param name="configuration">The SDK configuration to map.</param>
    /// <returns>The domain summary.</returns>
    public static LambdaFunctionSummary ToSummary(FunctionConfiguration configuration)
        => new(
            configuration.FunctionName ?? string.Empty,
            configuration.Runtime?.Value ?? string.Empty,
            configuration.Description ?? string.Empty,
            configuration.LastModified ?? string.Empty,
            configuration.MemorySize ?? 0,
            configuration.Timeout ?? 0);

    /// <summary>
    /// Map an AWS function configuration to a detail view.
    /// </summary>
    /// <param name="configuration">The SDK configuration to map.</param>
    /// <returns>The domain detail.</returns>
    public static LambdaFunctionDetail ToDetail(FunctionConfiguration configuration)
        => new(
            configuration.FunctionName ?? string.Empty,
            configuration.FunctionArn ?? string.Empty,
            configuration.Runtime?.Value ?? string.Empty,
            configuration.Handler ?? string.Empty,
            configuration.Description ?? string.Empty,
            configuration.LastModified ?? string.Empty,
            configuration.MemorySize ?? 0,
            configuration.Timeout ?? 0,
            configuration.Role ?? string.Empty);

    /// <summary>
    /// Map an AWS event source mapping configuration to its domain representation.
    /// </summary>
    /// <param name="configuration">The SDK event source mapping to map.</param>
    /// <returns>The domain event source mapping.</returns>
    public static LambdaEventSourceMapping ToEventSourceMapping(EventSourceMappingConfiguration configuration)
        => new(
            configuration.UUID ?? string.Empty,
            configuration.EventSourceArn ?? string.Empty,
            configuration.FunctionArn ?? string.Empty,
            configuration.State ?? string.Empty,
            configuration.BatchSize ?? 0,
            configuration.LastModified?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);

    /// <summary>
    /// Map an AWS CloudWatch Logs filtered event to its domain representation.
    /// </summary>
    /// <param name="logEvent">The SDK log event to map.</param>
    /// <returns>The domain log event.</returns>
    public static LambdaLogEvent ToLogEvent(FilteredLogEvent logEvent)
        => new(
            logEvent.Timestamp.HasValue
                ? DateTimeOffset.FromUnixTimeMilliseconds(logEvent.Timestamp.Value).ToString("O", CultureInfo.InvariantCulture)
                : string.Empty,
            logEvent.Message ?? string.Empty,
            logEvent.LogStreamName ?? string.Empty);

    /// <summary>
    /// Map an AWS layer version reference to its domain representation, deriving the layer name and
    /// version from the layer ARN (<c>arn:aws:lambda:&lt;region&gt;:&lt;account&gt;:layer:&lt;name&gt;:&lt;version&gt;</c>).
    /// </summary>
    /// <param name="layer">The SDK layer reference to map.</param>
    /// <returns>The domain layer.</returns>
    public static LambdaLayer ToLayer(Layer layer)
    {
        var arn = layer.Arn ?? string.Empty;
        var segments = arn.Split(':');
        var name = segments.Length > 7 ? segments[6] : string.Empty;
        var version = segments.Length > 7 ? segments[7] : string.Empty;
        return new LambdaLayer(arn, name, version);
    }
}
