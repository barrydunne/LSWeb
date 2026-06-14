using System.Diagnostics.CodeAnalysis;
using Amazon.S3;
using Amazon.S3.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Foundation.Infrastructure.Aws;
using S3Bucket = Foundation.Domain.S3.S3Bucket;

namespace Foundation.Infrastructure.S3;

/// <summary>
/// Reads and manages S3 buckets through the resilient AWS gateway so the same code works against
/// LocalStack or real AWS. All access flows through <see cref="IAwsGateway"/>, which records
/// capability and converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class S3ClientAdapter : IS3Client
{
    private const string ServiceKey = "s3";

    private readonly IAwsGateway _gateway;

    public S3ClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<S3Bucket>>> ListBucketsAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonS3Client, IReadOnlyList<S3Bucket>>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.ListBucketsAsync(new ListBucketsRequest(), token);
                return (response.Buckets ?? [])
                    .Select(S3BucketMapper.ToBucket)
                    .ToList();
            },
            cancellationToken);

    public async Task<Result> CreateBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new PutBucketRequest { BucketName = bucketName };

                // Every region other than us-east-1 requires an explicit LocationConstraint;
                // without it the SDK/LocalStack rejects the call with IllegalLocationConstraintException.
                var region = client.Config.AuthenticationRegion;
                if (!string.IsNullOrEmpty(region)
                    && !string.Equals(region, "us-east-1", StringComparison.OrdinalIgnoreCase))
                    request.BucketRegionName = region;

                await client.PutBucketAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteBucketAsync(new DeleteBucketRequest { BucketName = bucketName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<Foundation.Domain.S3.S3ObjectListing>> ListObjectsAsync(
        string bucketName, string prefix, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonS3Client, Foundation.Domain.S3.S3ObjectListing>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.ListObjectsV2Async(
                    new ListObjectsV2Request { BucketName = bucketName, Prefix = prefix, Delimiter = "/" }, token);
                var prefixes = (response.CommonPrefixes ?? []).ToList();
                var objects = (response.S3Objects ?? [])
                    .Where(o => o.Key != prefix)
                    .Select(S3BucketMapper.ToObject)
                    .ToList();
                return new Foundation.Domain.S3.S3ObjectListing(prefixes, objects);
            },
            cancellationToken);

    public async Task<Result> CreateFolderAsync(string bucketName, string folderKey, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutObjectAsync(
                    new PutObjectRequest { BucketName = bucketName, Key = folderKey, ContentBody = string.Empty }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> UploadObjectAsync(
        string bucketName, string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutObjectAsync(
                    new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        InputStream = content,
                        ContentType = contentType,
                        AutoCloseStream = false,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<Foundation.Domain.S3.S3ObjectContent>> DownloadObjectAsync(
        string bucketName, string key, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonS3Client, Foundation.Domain.S3.S3ObjectContent>(
            ServiceKey,
            async (client, token) =>
            {
                using var response = await client.GetObjectAsync(
                    new GetObjectRequest { BucketName = bucketName, Key = key }, token);
                using var buffer = new MemoryStream();
                await response.ResponseStream.CopyToAsync(buffer, token);
                var contentType = string.IsNullOrEmpty(response.Headers.ContentType)
                    ? "application/octet-stream"
                    : response.Headers.ContentType;
                return new Foundation.Domain.S3.S3ObjectContent(buffer.ToArray(), contentType);
            },
            cancellationToken);

    public Task<Result<Foundation.Domain.S3.S3ObjectPreview>> PreviewObjectAsync(
        string bucketName, string key, int maxBytes, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonS3Client, Foundation.Domain.S3.S3ObjectPreview>(
            ServiceKey,
            async (client, token) =>
            {
                using var response = await client.GetObjectAsync(
                    new GetObjectRequest { BucketName = bucketName, Key = key }, token);
                var totalSize = response.Headers.ContentLength;
                var contentType = string.IsNullOrEmpty(response.Headers.ContentType)
                    ? "application/octet-stream"
                    : response.Headers.ContentType;

                using var buffer = new MemoryStream();
                var chunk = new byte[8192];
                while (buffer.Length < maxBytes)
                {
                    var toRead = (int)Math.Min(chunk.Length, maxBytes - buffer.Length);
                    var read = await response.ResponseStream.ReadAsync(chunk.AsMemory(0, toRead), token);
                    if (read == 0)
                        break;

                    buffer.Write(chunk, 0, read);
                }

                var content = buffer.ToArray();
                var truncated = totalSize > content.Length;
                return new Foundation.Domain.S3.S3ObjectPreview(content, contentType, totalSize, truncated);
            },
            cancellationToken);

    public Task<Result<string>> GeneratePresignedUrlAsync(
        string bucketName, string key, TimeSpan expiresIn, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonS3Client, string>(
            ServiceKey,
            (client, _) => client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiresIn),
            }),
            cancellationToken);

    public async Task<Result> DeleteObjectAsync(string bucketName, string key, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = bucketName, Key = key }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> CopyObjectAsync(
        string sourceBucketName,
        string sourceKey,
        string destinationBucketName,
        string destinationKey,
        CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CopyObjectAsync(
                    new CopyObjectRequest
                    {
                        SourceBucket = sourceBucketName,
                        SourceKey = sourceKey,
                        DestinationBucket = destinationBucketName,
                        DestinationKey = destinationKey,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<Foundation.Domain.S3.S3ObjectMetadata>> GetObjectMetadataAsync(
        string bucketName, string key, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonS3Client, Foundation.Domain.S3.S3ObjectMetadata>(
            ServiceKey,
            async (client, token) =>
            {
                var head = await client.GetObjectMetadataAsync(
                    new GetObjectMetadataRequest { BucketName = bucketName, Key = key }, token);
                var tagging = await client.GetObjectTaggingAsync(
                    new GetObjectTaggingRequest { BucketName = bucketName, Key = key }, token);

                var userMetadata = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var metadataKey in head.Metadata.Keys)
                {
                    var name = metadataKey.StartsWith("x-amz-meta-", StringComparison.OrdinalIgnoreCase)
                        ? metadataKey["x-amz-meta-".Length..]
                        : metadataKey;
                    userMetadata[name] = head.Metadata[metadataKey] ?? string.Empty;
                }

                var tags = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var tag in tagging.Tagging ?? [])
                    tags[tag.Key] = tag.Value;

                var contentType = string.IsNullOrEmpty(head.Headers.ContentType)
                    ? "application/octet-stream"
                    : head.Headers.ContentType;
                var lastModified = head.LastModified?.ToString("O") ?? string.Empty;
                return new Foundation.Domain.S3.S3ObjectMetadata(
                    contentType, head.Headers.ContentLength, lastModified, head.ETag ?? string.Empty, userMetadata, tags);
            },
            cancellationToken);

    public async Task<Result> UpdateObjectTagsAsync(
        string bucketName, string key, IReadOnlyDictionary<string, string> tags, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var tagSet = tags
                    .Select(tag => new Tag { Key = tag.Key, Value = tag.Value })
                    .ToList();
                await client.PutObjectTaggingAsync(
                    new PutObjectTaggingRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        Tagging = new Tagging { TagSet = tagSet },
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<Foundation.Domain.S3.S3BucketConfiguration>> GetBucketConfigurationAsync(
        string bucketName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonS3Client, Foundation.Domain.S3.S3BucketConfiguration>(
            ServiceKey,
            async (client, token) =>
            {
                var versioning = await ReadVersioningAsync(client, bucketName, token);
                var (algorithm, keyId) = await ReadEncryptionAsync(client, bucketName, token);
                var lifecycle = await ReadLifecycleAsync(client, bucketName, token);
                var notifications = await ReadNotificationsAsync(client, bucketName, token);
                var policy = await ReadPolicyAsync(client, bucketName, token);

                return new Foundation.Domain.S3.S3BucketConfiguration(
                    versioning, algorithm, keyId, lifecycle, notifications, policy);
            },
            cancellationToken);

    private static async Task<string> ReadVersioningAsync(
        AmazonS3Client client, string bucketName, CancellationToken cancellationToken)
    {
        var response = await client.GetBucketVersioningAsync(
            new GetBucketVersioningRequest { BucketName = bucketName }, cancellationToken);
        var status = response.VersioningConfig?.Status?.Value;
        return string.IsNullOrEmpty(status) ? "Disabled" : status;
    }

    private static async Task<(string Algorithm, string KeyId)> ReadEncryptionAsync(
        AmazonS3Client client, string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetBucketEncryptionAsync(
                new GetBucketEncryptionRequest { BucketName = bucketName }, cancellationToken);
            var rule = (response.ServerSideEncryptionConfiguration?.ServerSideEncryptionRules ?? [])
                .FirstOrDefault();
            var byDefault = rule?.ServerSideEncryptionByDefault;
            var algorithm = byDefault?.ServerSideEncryptionAlgorithm?.Value ?? string.Empty;
            var keyId = byDefault?.ServerSideEncryptionKeyManagementServiceKeyId ?? string.Empty;
            return (algorithm, keyId);
        }
        catch (AmazonS3Exception)
        {
            return (string.Empty, string.Empty);
        }
    }

    private static async Task<IReadOnlyList<Foundation.Domain.S3.S3LifecycleRule>> ReadLifecycleAsync(
        AmazonS3Client client, string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetLifecycleConfigurationAsync(
                new GetLifecycleConfigurationRequest { BucketName = bucketName }, cancellationToken);
            return (response.Configuration?.Rules ?? [])
                .Select(rule => new Foundation.Domain.S3.S3LifecycleRule(
                    rule.Id ?? string.Empty,
                    rule.Status?.Value ?? string.Empty,
                    LifecyclePrefix(rule.Filter)))
                .ToList();
        }
        catch (AmazonS3Exception)
        {
            return [];
        }
    }

    private static string LifecyclePrefix(LifecycleFilter? filter)
    {
        if (filter is null)
            return string.Empty;

        // SDK v4 deserializes <Filter><Prefix> into the predicate, not the legacy Prefix property.
        return filter.LifecycleFilterPredicate switch
        {
            LifecyclePrefixPredicate prefix => prefix.Prefix ?? string.Empty,
            LifecycleAndOperator and => and.Operands?
                .OfType<LifecyclePrefixPredicate>()
                .Select(operand => operand.Prefix)
                .FirstOrDefault() ?? string.Empty,
            _ => filter.Prefix ?? string.Empty,
        };
    }

    private static async Task<IReadOnlyList<Foundation.Domain.S3.S3NotificationConfiguration>> ReadNotificationsAsync(
        AmazonS3Client client, string bucketName, CancellationToken cancellationToken)
    {
        var response = await client.GetBucketNotificationAsync(
            new GetBucketNotificationRequest { BucketName = bucketName }, cancellationToken);

        var notifications = new List<Foundation.Domain.S3.S3NotificationConfiguration>();
        foreach (var lambda in response.LambdaFunctionConfigurations ?? [])
            notifications.Add(new Foundation.Domain.S3.S3NotificationConfiguration(
                "Lambda", lambda.FunctionArn ?? string.Empty, EventNames(lambda.Events),
                FilterValue(lambda.Filter, "prefix"), FilterValue(lambda.Filter, "suffix")));
        foreach (var queue in response.QueueConfigurations ?? [])
            notifications.Add(new Foundation.Domain.S3.S3NotificationConfiguration(
                "Queue", queue.Queue ?? string.Empty, EventNames(queue.Events),
                FilterValue(queue.Filter, "prefix"), FilterValue(queue.Filter, "suffix")));
        foreach (var topic in response.TopicConfigurations ?? [])
            notifications.Add(new Foundation.Domain.S3.S3NotificationConfiguration(
                "Topic", topic.Topic ?? string.Empty, EventNames(topic.Events),
                FilterValue(topic.Filter, "prefix"), FilterValue(topic.Filter, "suffix")));

        return notifications;
    }

    private static async Task<string> ReadPolicyAsync(
        AmazonS3Client client, string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetBucketPolicyAsync(
                new GetBucketPolicyRequest { BucketName = bucketName }, cancellationToken);
            var policy = response.Policy ?? string.Empty;

            // A bucket policy is always a JSON document. LocalStack may return an
            // error document (e.g. NoSuchBucketPolicy) in the body instead of throwing,
            // so treat any non-JSON payload as "no policy configured".
            return policy.TrimStart().StartsWith('{') ? policy : string.Empty;
        }
        catch (AmazonS3Exception)
        {
            return string.Empty;
        }
    }

    private static List<string> EventNames(List<EventType>? events)
        => (events ?? [])
            .Select(eventType => eventType.Value)
            .ToList();

    private static string FilterValue(Filter? filter, string ruleName)
        => (filter?.S3KeyFilter?.FilterRules ?? [])
            .Where(rule => string.Equals(rule.Name, ruleName, StringComparison.OrdinalIgnoreCase))
            .Select(rule => rule.Value ?? string.Empty)
            .FirstOrDefault() ?? string.Empty;

    public Task<Result<Foundation.Domain.S3.S3BucketStorageSummary>> GetBucketStorageSummaryAsync(
        string bucketName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonS3Client, Foundation.Domain.S3.S3BucketStorageSummary>(
            ServiceKey,
            async (client, token) =>
            {
                long objectCount = 0;
                long totalSizeBytes = 0;
                string? continuationToken = null;

                do
                {
                    var response = await client.ListObjectsV2Async(
                        new ListObjectsV2Request
                        {
                            BucketName = bucketName,
                            ContinuationToken = continuationToken,
                        },
                        token);

                    foreach (var s3Object in response.S3Objects ?? [])
                    {
                        // Skip zero-byte folder marker keys so the summary reflects real files.
                        if ((s3Object.Key ?? string.Empty).EndsWith('/'))
                            continue;

                        objectCount++;
                        totalSizeBytes += s3Object.Size ?? 0;
                    }

                    continuationToken = response.IsTruncated == true ? response.NextContinuationToken : null;
                }
                while (continuationToken is not null);

                return new Foundation.Domain.S3.S3BucketStorageSummary(objectCount, totalSizeBytes);
            },
            cancellationToken);

    public async Task<Result> PutBucketPolicyAsync(string bucketName, string policy, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutBucketPolicyAsync(
                    new PutBucketPolicyRequest { BucketName = bucketName, Policy = policy }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteBucketPolicyAsync(string bucketName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteBucketPolicyAsync(
                    new DeleteBucketPolicyRequest { BucketName = bucketName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> SetBucketVersioningAsync(string bucketName, bool enabled, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutBucketVersioningAsync(
                    new PutBucketVersioningRequest
                    {
                        BucketName = bucketName,
                        VersioningConfig = new S3BucketVersioningConfig
                        {
                            Status = enabled ? VersionStatus.Enabled : VersionStatus.Suspended,
                        },
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<Foundation.Domain.S3.S3ObjectVersion>>> ListObjectVersionsAsync(
        string bucketName, string prefix, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonS3Client, IReadOnlyList<Foundation.Domain.S3.S3ObjectVersion>>(
            ServiceKey,
            async (client, token) =>
            {
                var versions = new List<Foundation.Domain.S3.S3ObjectVersion>();
                string? keyMarker = null;
                string? versionIdMarker = null;

                do
                {
                    var response = await client.ListVersionsAsync(
                        new ListVersionsRequest
                        {
                            BucketName = bucketName,
                            Prefix = prefix,
                            KeyMarker = keyMarker,
                            VersionIdMarker = versionIdMarker,
                        },
                        token);

                    foreach (var version in response.Versions ?? [])
                        versions.Add(new Foundation.Domain.S3.S3ObjectVersion(
                            version.Key ?? string.Empty,
                            version.VersionId ?? string.Empty,
                            version.IsLatest ?? false,
                            version.IsDeleteMarker ?? false,
                            version.Size ?? 0,
                            version.LastModified?.ToString("O") ?? string.Empty));

                    if (response.IsTruncated == true)
                    {
                        keyMarker = response.NextKeyMarker;
                        versionIdMarker = response.NextVersionIdMarker;
                    }
                    else
                    {
                        keyMarker = null;
                        versionIdMarker = null;
                    }
                }
                while (!string.IsNullOrEmpty(keyMarker));

                return versions;
            },
            cancellationToken);

    public async Task<Result> DeleteObjectVersionAsync(
        string bucketName, string key, string versionId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteObjectAsync(
                    new DeleteObjectRequest { BucketName = bucketName, Key = key, VersionId = versionId }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> PutBucketNotificationsAsync(
        string bucketName, IReadOnlyList<Foundation.Domain.S3.S3NotificationConfiguration> notifications, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new PutBucketNotificationRequest
                {
                    BucketName = bucketName,
                    LambdaFunctionConfigurations = [],
                    QueueConfigurations = [],
                    TopicConfigurations = [],
                };

                foreach (var notification in notifications)
                {
                    var events = notification.Events.Select(EventType.FindValue).ToList();
                    var filter = BuildFilter(notification.Prefix, notification.Suffix);

                    switch (notification.Type)
                    {
                        case "Queue":
                            request.QueueConfigurations.Add(new QueueConfiguration
                            {
                                Queue = notification.TargetArn,
                                Events = events,
                                Filter = filter,
                            });
                            break;
                        case "Topic":
                            request.TopicConfigurations.Add(new TopicConfiguration
                            {
                                Topic = notification.TargetArn,
                                Events = events,
                                Filter = filter,
                            });
                            break;
                        default:
                            request.LambdaFunctionConfigurations.Add(new LambdaFunctionConfiguration
                            {
                                FunctionArn = notification.TargetArn,
                                Events = events,
                                Filter = filter,
                            });
                            break;
                    }
                }

                await client.PutBucketNotificationAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static Filter? BuildFilter(string prefix, string suffix)
    {
        var rules = new List<FilterRule>();
        if (!string.IsNullOrEmpty(prefix))
            rules.Add(new FilterRule { Name = "prefix", Value = prefix });
        if (!string.IsNullOrEmpty(suffix))
            rules.Add(new FilterRule { Name = "suffix", Value = suffix });

        return rules.Count == 0 ? null : new Filter { S3KeyFilter = new S3KeyFilter { FilterRules = rules } };
    }
}
