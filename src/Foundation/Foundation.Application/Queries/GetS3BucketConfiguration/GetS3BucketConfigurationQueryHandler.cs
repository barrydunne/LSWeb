using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetS3BucketConfiguration;

internal sealed partial class GetS3BucketConfigurationQueryHandler
    : IQueryHandler<GetS3BucketConfigurationQuery, GetS3BucketConfigurationQueryResult>
{
    private readonly IS3Client _client;
    private readonly ILogger _logger;

    public GetS3BucketConfigurationQueryHandler(IS3Client client, ILogger<GetS3BucketConfigurationQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetS3BucketConfigurationQueryResult>> Handle(
        GetS3BucketConfigurationQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName);
        var configuration = await _client.GetBucketConfigurationAsync(request.BucketName, cancellationToken);
        LogHandled(configuration.IsSuccess);

        if (!configuration.IsSuccess)
        {
            Result<GetS3BucketConfigurationQueryResult> failure = configuration.Error!.Value;
            return failure;
        }

        var value = configuration.Value;
        var lifecycleRules = value.LifecycleRules
            .OrderBy(rule => rule.Id, StringComparer.Ordinal)
            .Select(rule => new S3LifecycleRuleResult(rule.Id, rule.Status, rule.Prefix))
            .ToList();
        var notifications = value.Notifications
            .Select(notification => new S3NotificationResult(
                notification.Type, notification.TargetArn, notification.Events,
                notification.Prefix, notification.Suffix))
            .ToList();

        return new GetS3BucketConfigurationQueryResult(
            value.VersioningStatus,
            value.EncryptionAlgorithm,
            value.EncryptionKeyId,
            lifecycleRules,
            notifications,
            value.Policy);
    }

    [LoggerMessage(LogLevel.Trace, "Reading S3 bucket configuration for {BucketName}.")]
    private partial void LogHandling(string bucketName);

    [LoggerMessage(LogLevel.Trace, "S3 bucket configuration read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
