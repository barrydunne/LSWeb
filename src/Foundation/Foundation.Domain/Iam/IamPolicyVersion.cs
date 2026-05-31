namespace Foundation.Domain.Iam;

/// <summary>
/// A single version of an IAM managed policy.
/// </summary>
/// <param name="VersionId">The identifier of the version, such as <c>v1</c>.</param>
/// <param name="IsDefaultVersion">Whether this version is the policy's current default version.</param>
/// <param name="CreateDate">When the version was created, if known.</param>
public sealed record IamPolicyVersion(
    string VersionId,
    bool IsDefaultVersion,
    DateTimeOffset? CreateDate);
