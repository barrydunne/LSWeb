namespace Foundation.Domain.Iam;

/// <summary>
/// The full detail of an IAM user, including the groups, policies, and access keys attached to it.
/// </summary>
/// <param name="UserName">The name of the user.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the user.</param>
/// <param name="UserId">The stable identifier assigned to the user.</param>
/// <param name="Path">The path under which the user is organised.</param>
/// <param name="CreateDate">When the user was created, if known.</param>
/// <param name="Groups">The names of the groups the user belongs to.</param>
/// <param name="AttachedPolicies">The managed policies attached to the user.</param>
/// <param name="InlinePolicyNames">The names of the inline policies embedded in the user.</param>
/// <param name="AccessKeys">The access keys belonging to the user.</param>
/// <param name="Tags">The key/value tags attached to the user.</param>
/// <param name="PermissionsBoundaryArn">The ARN of the policy used as the user's permissions boundary, if one is set.</param>
public sealed record IamUserDetail(
    string UserName,
    string Arn,
    string UserId,
    string Path,
    DateTimeOffset? CreateDate,
    IReadOnlyList<string> Groups,
    IReadOnlyList<IamAttachedPolicy> AttachedPolicies,
    IReadOnlyList<string> InlinePolicyNames,
    IReadOnlyList<IamAccessKey> AccessKeys,
    IReadOnlyList<IamTag> Tags,
    string? PermissionsBoundaryArn);

/// <summary>
/// A managed policy attached to an IAM principal.
/// </summary>
/// <param name="PolicyName">The name of the policy.</param>
/// <param name="PolicyArn">The Amazon Resource Name that uniquely identifies the policy.</param>
public sealed record IamAttachedPolicy(string PolicyName, string PolicyArn);

/// <summary>
/// An access key belonging to an IAM user along with its most recent usage.
/// </summary>
/// <param name="AccessKeyId">The identifier of the access key.</param>
/// <param name="Status">The status of the key, such as Active or Inactive.</param>
/// <param name="CreateDate">When the key was created, if known.</param>
/// <param name="LastUsedDate">When the key was last used, if ever.</param>
/// <param name="LastUsedService">The service the key was last used against, if known.</param>
/// <param name="LastUsedRegion">The region the key was last used in, if known.</param>
public sealed record IamAccessKey(
    string AccessKeyId,
    string Status,
    DateTimeOffset? CreateDate,
    DateTimeOffset? LastUsedDate,
    string? LastUsedService,
    string? LastUsedRegion);
