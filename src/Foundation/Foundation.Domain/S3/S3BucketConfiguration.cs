namespace Foundation.Domain.S3;

/// <summary>
/// The resolved configuration of a single S3 bucket: versioning, default encryption, lifecycle
/// rules, event notifications and the access policy. Aspects that are not configured on the bucket
/// are represented by their empty/default value rather than an error so the whole view always renders.
/// </summary>
/// <param name="VersioningStatus">The bucket versioning status: <c>Enabled</c>, <c>Suspended</c> or <c>Disabled</c>.</param>
/// <param name="EncryptionAlgorithm">The default server-side encryption algorithm (for example <c>AES256</c> or <c>aws:kms</c>); empty when none is configured.</param>
/// <param name="EncryptionKeyId">The KMS key id used by default encryption; empty when not applicable.</param>
/// <param name="LifecycleRules">The lifecycle rules defined on the bucket; empty when none.</param>
/// <param name="Notifications">The event notification configurations defined on the bucket; empty when none.</param>
/// <param name="Policy">The bucket access policy document as raw JSON; empty when none is configured.</param>
public record S3BucketConfiguration(
    string VersioningStatus,
    string EncryptionAlgorithm,
    string EncryptionKeyId,
    IReadOnlyList<S3LifecycleRule> LifecycleRules,
    IReadOnlyList<S3NotificationConfiguration> Notifications,
    string Policy);

/// <summary>
/// A summary of a single S3 lifecycle rule.
/// </summary>
/// <param name="Id">The rule identifier; empty when the bucket did not name the rule.</param>
/// <param name="Status">The rule status: <c>Enabled</c> or <c>Disabled</c>.</param>
/// <param name="Prefix">The object key prefix the rule applies to; empty when the rule applies to the whole bucket.</param>
public record S3LifecycleRule(string Id, string Status, string Prefix);

/// <summary>
/// A single S3 event notification configuration and the cross-resource target it delivers to.
/// </summary>
/// <param name="Type">The target resource type: <c>Lambda</c>, <c>Queue</c> or <c>Topic</c>.</param>
/// <param name="TargetArn">The ARN of the target Lambda function, SQS queue or SNS topic.</param>
/// <param name="Events">The S3 event names that trigger the notification.</param>
public record S3NotificationConfiguration(string Type, string TargetArn, IReadOnlyList<string> Events);
