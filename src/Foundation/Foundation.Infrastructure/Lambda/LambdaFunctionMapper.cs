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
    /// Map an AWS get-function response to the read-only code view, combining the package metadata from
    /// the configuration with the download or image location from the code block. For a zip package the
    /// location is the package download URL; for an image package it is the resolved image URI.
    /// </summary>
    /// <param name="configuration">The SDK configuration to map.</param>
    /// <param name="code">The SDK code location to map; may be <see langword="null"/>.</param>
    /// <returns>The domain code view.</returns>
    public static LambdaFunctionCode ToCode(FunctionConfiguration configuration, FunctionCodeLocation? code)
    {
        var imageUri = code?.ImageUri ?? string.Empty;
        var resolvedImageUri = code?.ResolvedImageUri ?? string.Empty;
        var downloadLocation = code?.Location ?? string.Empty;
        var location = imageUri.Length == 0
            ? downloadLocation
            : resolvedImageUri.Length > 0 ? resolvedImageUri : imageUri;
        return new LambdaFunctionCode(
            configuration.FunctionName ?? string.Empty,
            configuration.Runtime?.Value ?? string.Empty,
            configuration.Handler ?? string.Empty,
            configuration.PackageType?.Value ?? string.Empty,
            configuration.CodeSize ?? 0,
            configuration.CodeSha256 ?? string.Empty,
            code?.RepositoryType ?? string.Empty,
            location,
            imageUri);
    }

    /// <summary>
    /// Map an AWS function URL configuration to its domain representation.
    /// </summary>
    /// <param name="functionUrl">The HTTPS endpoint that invokes the function.</param>
    /// <param name="authType">The authentication mode reported by AWS.</param>
    /// <param name="creationTime">The creation timestamp reported by AWS.</param>
    /// <param name="lastModifiedTime">The last-modified timestamp reported by AWS.</param>
    /// <returns>The domain function URL.</returns>
    public static LambdaFunctionUrl ToFunctionUrl(string? functionUrl, string? authType, string? creationTime, string? lastModifiedTime)
        => new(
            functionUrl ?? string.Empty,
            authType ?? string.Empty,
            creationTime ?? string.Empty,
            lastModifiedTime ?? string.Empty);

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
