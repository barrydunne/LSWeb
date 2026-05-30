using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.S3;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListLambdaEventSourceMappings;

internal sealed partial class ListLambdaEventSourceMappingsQueryHandler : IQueryHandler<ListLambdaEventSourceMappingsQuery, ListLambdaEventSourceMappingsQueryResult>
{
    private const string FunctionArnMarker = ":function:";

    private readonly ILambdaClient _client;
    private readonly IS3Client _s3Client;
    private readonly ILogger _logger;

    public ListLambdaEventSourceMappingsQueryHandler(
        ILambdaClient client, IS3Client s3Client, ILogger<ListLambdaEventSourceMappingsQueryHandler> logger)
    {
        _client = client;
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<Result<ListLambdaEventSourceMappingsQueryResult>> Handle(ListLambdaEventSourceMappingsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var mappings = await _client.ListEventSourceMappingsAsync(request.FunctionName, cancellationToken);
        if (!mappings.IsSuccess)
        {
            LogHandled(false);
            Result<ListLambdaEventSourceMappingsQueryResult> failure = mappings.Error!.Value;
            return failure;
        }

        var policyTriggers = await _client.ListS3TriggersAsync(request.FunctionName, cancellationToken);
        if (!policyTriggers.IsSuccess)
        {
            LogHandled(false);
            Result<ListLambdaEventSourceMappingsQueryResult> failure = policyTriggers.Error!.Value;
            return failure;
        }

        var bucketTriggers = await DiscoverBucketTriggersAsync(request.FunctionName, cancellationToken);
        if (!bucketTriggers.IsSuccess)
        {
            LogHandled(false);
            Result<ListLambdaEventSourceMappingsQueryResult> failure = bucketTriggers.Error!.Value;
            return failure;
        }

        LogHandled(true);

        var orderedMappings = mappings.Value
            .OrderBy(_ => _.EventSourceArn, StringComparer.Ordinal)
            .ToList();
        var orderedTriggers = MergeTriggers(policyTriggers.Value, bucketTriggers.Value);

        return new ListLambdaEventSourceMappingsQueryResult(orderedMappings, orderedTriggers);
    }

    private async Task<Result<IReadOnlyList<LambdaS3Trigger>>> DiscoverBucketTriggersAsync(
        string functionName, CancellationToken cancellationToken)
    {
        var buckets = await _s3Client.ListBucketsAsync(cancellationToken);
        if (!buckets.IsSuccess)
            return buckets.Error!.Value;

        var triggers = new List<LambdaS3Trigger>();
        foreach (var bucket in buckets.Value)
        {
            var configuration = await _s3Client.GetBucketConfigurationAsync(bucket.Name, cancellationToken);
            if (!configuration.IsSuccess)
                return configuration.Error!.Value;

            var triggersFunction = configuration.Value.Notifications.Any(notification =>
                string.Equals(notification.Type, "Lambda", StringComparison.OrdinalIgnoreCase)
                && FunctionArnTargetsFunction(notification.TargetArn, functionName));
            if (triggersFunction)
                triggers.Add(new LambdaS3Trigger($"arn:aws:s3:::{bucket.Name}"));
        }

        return triggers;
    }

    private static List<LambdaS3Trigger> MergeTriggers(
        IReadOnlyList<LambdaS3Trigger> policyTriggers, IReadOnlyList<LambdaS3Trigger> bucketTriggers)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return policyTriggers
            .Concat(bucketTriggers)
            .Where(trigger => seen.Add(trigger.BucketArn))
            .OrderBy(trigger => trigger.BucketArn, StringComparer.Ordinal)
            .ToList();
    }

    private static bool FunctionArnTargetsFunction(string targetArn, string functionName)
    {
        if (string.IsNullOrEmpty(targetArn))
            return false;

        if (string.Equals(targetArn, functionName, StringComparison.OrdinalIgnoreCase))
            return true;

        var index = targetArn.IndexOf(FunctionArnMarker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            return false;

        var name = targetArn[(index + FunctionArnMarker.Length)..];
        var qualifier = name.IndexOf(':');
        if (qualifier >= 0)
            name = name[..qualifier];

        return string.Equals(name, functionName, StringComparison.OrdinalIgnoreCase);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Lambda event source mappings for {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda event source mapping listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
