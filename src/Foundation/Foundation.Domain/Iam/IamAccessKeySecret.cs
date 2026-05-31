namespace Foundation.Domain.Iam;

/// <summary>
/// A newly created IAM access key, including the secret access key which is only ever returned once
/// at creation time and must be surfaced to the caller with a copy-once warning.
/// </summary>
/// <param name="AccessKeyId">The identifier of the access key.</param>
/// <param name="SecretAccessKey">The secret access key, returned only at creation time.</param>
/// <param name="Status">The status of the access key (for example <c>Active</c>).</param>
/// <param name="CreateDate">The moment the access key was created, if reported by the backend.</param>
public sealed record IamAccessKeySecret(
    string AccessKeyId,
    string SecretAccessKey,
    string Status,
    DateTimeOffset? CreateDate);
