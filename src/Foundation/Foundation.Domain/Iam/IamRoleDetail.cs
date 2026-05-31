namespace Foundation.Domain.Iam;

/// <summary>
/// The full detail of an IAM role, including its trust policy and the policies attached to it.
/// </summary>
/// <param name="RoleName">The name of the role.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the role.</param>
/// <param name="RoleId">The stable identifier assigned to the role.</param>
/// <param name="Path">The path under which the role is organised.</param>
/// <param name="CreateDate">When the role was created, if known.</param>
/// <param name="Description">The description of the role, if one was supplied.</param>
/// <param name="MaxSessionDuration">The maximum session duration, in seconds, if known.</param>
/// <param name="AssumeRolePolicyDocument">The trust policy JSON document that controls who may assume the role.</param>
/// <param name="AttachedPolicies">The managed policies attached to the role.</param>
/// <param name="InlinePolicies">The inline policies embedded in the role, with their documents.</param>
/// <param name="Tags">The key/value tags attached to the role.</param>
/// <param name="PermissionsBoundaryArn">The ARN of the policy used as the role's permissions boundary, if one is set.</param>
public sealed record IamRoleDetail(
    string RoleName,
    string Arn,
    string RoleId,
    string Path,
    DateTimeOffset? CreateDate,
    string? Description,
    int? MaxSessionDuration,
    string AssumeRolePolicyDocument,
    IReadOnlyList<IamAttachedPolicy> AttachedPolicies,
    IReadOnlyList<IamInlinePolicy> InlinePolicies,
    IReadOnlyList<IamTag> Tags,
    string? PermissionsBoundaryArn);
