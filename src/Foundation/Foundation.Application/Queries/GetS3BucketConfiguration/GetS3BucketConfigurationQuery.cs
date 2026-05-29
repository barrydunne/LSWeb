using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetS3BucketConfiguration;

/// <summary>
/// Read the configuration of a single S3 bucket: versioning, default encryption, lifecycle rules,
/// event notifications and the access policy.
/// </summary>
/// <param name="BucketName">The bucket to read the configuration of.</param>
public record GetS3BucketConfigurationQuery(string BucketName) : IQuery<GetS3BucketConfigurationQueryResult>;

/// <summary>
/// The resolved configuration of a single S3 bucket.
/// </summary>
/// <param name="VersioningStatus">The bucket versioning status: <c>Enabled</c>, <c>Suspended</c> or <c>Disabled</c>.</param>
/// <param name="EncryptionAlgorithm">The default server-side encryption algorithm; empty when none is configured.</param>
/// <param name="EncryptionKeyId">The KMS key id used by default encryption; empty when not applicable.</param>
/// <param name="LifecycleRules">The lifecycle rules defined on the bucket, ordered by id.</param>
/// <param name="Notifications">The event notification configurations defined on the bucket.</param>
/// <param name="Policy">The bucket access policy document as raw JSON; empty when none is configured.</param>
public record GetS3BucketConfigurationQueryResult(
    string VersioningStatus,
    string EncryptionAlgorithm,
    string EncryptionKeyId,
    IReadOnlyList<S3LifecycleRuleResult> LifecycleRules,
    IReadOnlyList<S3NotificationResult> Notifications,
    string Policy);

/// <summary>
/// A summary of a single S3 lifecycle rule.
/// </summary>
/// <param name="Id">The rule identifier; empty when the bucket did not name the rule.</param>
/// <param name="Status">The rule status: <c>Enabled</c> or <c>Disabled</c>.</param>
/// <param name="Prefix">The object key prefix the rule applies to; empty when the rule applies to the whole bucket.</param>
public record S3LifecycleRuleResult(string Id, string Status, string Prefix);

/// <summary>
/// A single S3 event notification configuration and the cross-resource target it delivers to.
/// </summary>
/// <param name="Type">The target resource type: <c>Lambda</c>, <c>Queue</c> or <c>Topic</c>.</param>
/// <param name="TargetArn">The ARN of the target Lambda function, SQS queue or SNS topic.</param>
/// <param name="Events">The S3 event names that trigger the notification.</param>
/// <param name="Prefix">The object key prefix the notification is filtered to; empty when no prefix filter is configured.</param>
/// <param name="Suffix">The object key suffix the notification is filtered to; empty when no suffix filter is configured.</param>
public record S3NotificationResult(
    string Type, string TargetArn, IReadOnlyList<string> Events, string Prefix, string Suffix);
