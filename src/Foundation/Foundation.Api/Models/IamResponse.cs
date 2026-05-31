namespace Foundation.Api.Models;

/// <summary>
/// The IAM users available on the configured backend.
/// </summary>
/// <param name="Users">The user summaries, ordered as returned by the backend.</param>
public sealed record IamUserListResponse(IReadOnlyList<IamUserSummaryResponse> Users);

/// <summary>
/// A concise view of an IAM user as it appears in a user list.
/// </summary>
/// <param name="UserName">The name of the user.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the user.</param>
/// <param name="UserId">The stable identifier assigned to the user.</param>
/// <param name="Path">The path under which the user is organised.</param>
/// <param name="CreateDate">When the user was created, if known.</param>
public sealed record IamUserSummaryResponse(
    string UserName,
    string Arn,
    string UserId,
    string Path,
    DateTimeOffset? CreateDate);

/// <summary>
/// The full detail of an IAM user, including the groups, policies, and access keys attached to it.
/// The secret portion of an access key is never included here.
/// </summary>
/// <param name="UserName">The name of the user.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the user.</param>
/// <param name="UserId">The stable identifier assigned to the user.</param>
/// <param name="Path">The path under which the user is organised.</param>
/// <param name="CreateDate">When the user was created, if known.</param>
/// <param name="Groups">The names of the groups the user belongs to.</param>
/// <param name="AttachedPolicies">The managed policies attached to the user.</param>
/// <param name="InlinePolicyNames">The names of the inline policies embedded in the user.</param>
/// <param name="AccessKeys">The access keys belonging to the user, without their secret values.</param>
/// <param name="Tags">The key/value tags applied to the user.</param>
/// <param name="PermissionsBoundaryArn">The ARN of the managed policy used as the user's permissions boundary, if any.</param>
public sealed record IamUserDetailResponse(
    string UserName,
    string Arn,
    string UserId,
    string Path,
    DateTimeOffset? CreateDate,
    IReadOnlyList<string> Groups,
    IReadOnlyList<IamAttachedPolicyResponse> AttachedPolicies,
    IReadOnlyList<string> InlinePolicyNames,
    IReadOnlyList<IamAccessKeyResponse> AccessKeys,
    IReadOnlyList<IamTagResponse> Tags,
    string? PermissionsBoundaryArn);

/// <summary>
/// A key/value tag applied to an IAM principal or managed policy.
/// </summary>
/// <param name="Key">The tag key.</param>
/// <param name="Value">The tag value.</param>
public sealed record IamTagResponse(string Key, string Value);

/// <summary>
/// A managed policy attached to an IAM principal.
/// </summary>
/// <param name="PolicyName">The name of the policy.</param>
/// <param name="PolicyArn">The Amazon Resource Name that uniquely identifies the policy.</param>
public sealed record IamAttachedPolicyResponse(string PolicyName, string PolicyArn);

/// <summary>
/// An access key belonging to an IAM user along with its most recent usage. The secret value of the
/// key is never included.
/// </summary>
/// <param name="AccessKeyId">The identifier of the access key.</param>
/// <param name="Status">The status of the key, such as Active or Inactive.</param>
/// <param name="CreateDate">When the key was created, if known.</param>
/// <param name="LastUsedDate">When the key was last used, if ever.</param>
/// <param name="LastUsedService">The service the key was last used against, if known.</param>
/// <param name="LastUsedRegion">The region the key was last used in, if known.</param>
public sealed record IamAccessKeyResponse(
    string AccessKeyId,
    string Status,
    DateTimeOffset? CreateDate,
    DateTimeOffset? LastUsedDate,
    string? LastUsedService,
    string? LastUsedRegion);

/// <summary>
/// A newly created access key including its secret value. The secret is returned only once, at the
/// moment of creation, and cannot be retrieved again.
/// </summary>
/// <param name="AccessKeyId">The identifier of the access key.</param>
/// <param name="SecretAccessKey">The secret value of the key. Copy it now; it cannot be shown again.</param>
/// <param name="Status">The status of the key, such as Active or Inactive.</param>
/// <param name="CreateDate">When the key was created, if known.</param>
public sealed record IamAccessKeySecretResponse(
    string AccessKeyId,
    string SecretAccessKey,
    string Status,
    DateTimeOffset? CreateDate);

/// <summary>
/// The IAM groups available on the configured backend.
/// </summary>
/// <param name="Groups">The group summaries, ordered as returned by the backend.</param>
public sealed record IamGroupListResponse(IReadOnlyList<IamGroupSummaryResponse> Groups);

/// <summary>
/// A concise view of an IAM group as it appears in a group list.
/// </summary>
/// <param name="GroupName">The name of the group.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the group.</param>
/// <param name="GroupId">The stable identifier assigned to the group.</param>
/// <param name="Path">The path under which the group is organised.</param>
/// <param name="CreateDate">When the group was created, if known.</param>
public sealed record IamGroupSummaryResponse(
    string GroupName,
    string Arn,
    string GroupId,
    string Path,
    DateTimeOffset? CreateDate);

/// <summary>
/// The full detail of an IAM group, including its members and the managed and inline policies
/// attached to it. Inline policies include their JSON documents.
/// </summary>
/// <param name="GroupName">The name of the group.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the group.</param>
/// <param name="GroupId">The stable identifier assigned to the group.</param>
/// <param name="Path">The path under which the group is organised.</param>
/// <param name="CreateDate">When the group was created, if known.</param>
/// <param name="Members">The names of the users that belong to the group.</param>
/// <param name="AttachedPolicies">The managed policies attached to the group.</param>
/// <param name="InlinePolicies">The inline policies embedded in the group, with their documents.</param>
public sealed record IamGroupDetailResponse(
    string GroupName,
    string Arn,
    string GroupId,
    string Path,
    DateTimeOffset? CreateDate,
    IReadOnlyList<string> Members,
    IReadOnlyList<IamAttachedPolicyResponse> AttachedPolicies,
    IReadOnlyList<IamInlinePolicyResponse> InlinePolicies);

/// <summary>
/// An inline policy embedded in an IAM principal, including its JSON document.
/// </summary>
/// <param name="PolicyName">The name of the inline policy.</param>
/// <param name="PolicyDocument">The JSON policy document.</param>
public sealed record IamInlinePolicyResponse(string PolicyName, string PolicyDocument);

/// <summary>
/// The IAM roles available on the configured backend.
/// </summary>
/// <param name="Roles">The role summaries, ordered as returned by the backend.</param>
public sealed record IamRoleListResponse(IReadOnlyList<IamRoleSummaryResponse> Roles);

/// <summary>
/// A concise view of an IAM role as it appears in a role list.
/// </summary>
/// <param name="RoleName">The name of the role.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the role.</param>
/// <param name="RoleId">The stable identifier assigned to the role.</param>
/// <param name="Path">The path under which the role is organised.</param>
/// <param name="CreateDate">When the role was created, if known.</param>
/// <param name="Description">The description of the role, if one was supplied.</param>
public sealed record IamRoleSummaryResponse(
    string RoleName,
    string Arn,
    string RoleId,
    string Path,
    DateTimeOffset? CreateDate,
    string? Description);

/// <summary>
/// The full detail of an IAM role, including its trust policy and the managed and inline policies
/// attached to it. Inline policies include their JSON documents.
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
/// <param name="Tags">The key/value tags applied to the role.</param>
/// <param name="PermissionsBoundaryArn">The ARN of the managed policy used as the role's permissions boundary, if any.</param>
public sealed record IamRoleDetailResponse(
    string RoleName,
    string Arn,
    string RoleId,
    string Path,
    DateTimeOffset? CreateDate,
    string? Description,
    int? MaxSessionDuration,
    string AssumeRolePolicyDocument,
    IReadOnlyList<IamAttachedPolicyResponse> AttachedPolicies,
    IReadOnlyList<IamInlinePolicyResponse> InlinePolicies,
    IReadOnlyList<IamTagResponse> Tags,
    string? PermissionsBoundaryArn);

/// <summary>
/// The details required to create a new IAM role.
/// </summary>
/// <param name="RoleName">The name of the role to create.</param>
/// <param name="AssumeRolePolicyDocument">The trust policy JSON document that controls who may assume the role.</param>
/// <param name="Path">An optional path under which to organise the role.</param>
/// <param name="Description">An optional description for the role.</param>
/// <param name="MaxSessionDuration">An optional maximum session duration, in seconds.</param>
public sealed record IamRoleCreateRequest(
    string RoleName,
    string AssumeRolePolicyDocument,
    string? Path,
    string? Description,
    int? MaxSessionDuration);

/// <summary>
/// The details to update on an existing IAM role. The description and maximum session duration are
/// always applied; the trust policy is updated only when a document is supplied.
/// </summary>
/// <param name="Description">The new description for the role.</param>
/// <param name="MaxSessionDuration">The new maximum session duration, in seconds.</param>
/// <param name="TrustPolicyDocument">An optional new trust policy JSON document.</param>
public sealed record IamRoleUpdateRequest(
    string? Description,
    int? MaxSessionDuration,
    string? TrustPolicyDocument);

/// <summary>
/// The resources that use an IAM role, such as Lambda functions whose execution role matches it.
/// </summary>
/// <param name="Consumers">The consumers that reference the role.</param>
public sealed record IamRoleConsumersResponse(IReadOnlyList<IamRoleConsumerResponse> Consumers);

/// <summary>
/// A single resource that uses an IAM role.
/// </summary>
/// <param name="ConsumerType">A human-readable description of the consumer kind, for example <c>Lambda function</c>.</param>
/// <param name="ResourceName">The name of the consuming resource, used as the navigation reference.</param>
/// <param name="ServiceKey">The service the consumer belongs to, for example <c>lambda</c>, used to route the reference.</param>
public sealed record IamRoleConsumerResponse(
    string ConsumerType,
    string ResourceName,
    string ServiceKey);

/// <summary>
/// The details required to create a new IAM user.
/// </summary>
/// <param name="UserName">The name of the user to create.</param>
/// <param name="Path">An optional path under which to organise the user.</param>
public sealed record IamUserCreateRequest(string UserName, string? Path);

/// <summary>
/// The details required to create a new IAM group.
/// </summary>
/// <param name="GroupName">The name of the group to create.</param>
/// <param name="Path">An optional path under which to organise the group.</param>
public sealed record IamGroupCreateRequest(string GroupName, string? Path);

/// <summary>
/// A request to add an IAM user to a group, identified from the group side.
/// </summary>
/// <param name="UserName">The name of the user to add to the group.</param>
public sealed record IamGroupMemberRequest(string UserName);

/// <summary>
/// A request to add an IAM user to a group.
/// </summary>
/// <param name="GroupName">The name of the group to add the user to.</param>
public sealed record IamGroupMembershipRequest(string GroupName);

/// <summary>
/// A request to attach a managed policy to an IAM user.
/// </summary>
/// <param name="PolicyArn">The Amazon Resource Name of the managed policy to attach.</param>
public sealed record IamAttachPolicyRequest(string PolicyArn);

/// <summary>
/// A request to store an inline policy document against an IAM user.
/// </summary>
/// <param name="PolicyDocument">The JSON policy document to store.</param>
public sealed record IamInlinePolicyRequest(string PolicyDocument);

/// <summary>
/// A request to change the status of an IAM access key.
/// </summary>
/// <param name="Status">The new status of the key, either Active or Inactive.</param>
public sealed record IamAccessKeyStatusRequest(string Status);

/// <summary>
/// The IAM managed policies available on the configured backend.
/// </summary>
/// <param name="Policies">The policy summaries, ordered as returned by the backend.</param>
public sealed record IamPolicyListResponse(IReadOnlyList<IamPolicySummaryResponse> Policies);

/// <summary>
/// A concise view of an IAM managed policy as it appears in a policy list.
/// </summary>
/// <param name="PolicyName">The name of the policy.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the policy.</param>
/// <param name="PolicyId">The stable identifier assigned to the policy.</param>
/// <param name="Path">The path under which the policy is organised.</param>
/// <param name="DefaultVersionId">The identifier of the version that is currently the default.</param>
/// <param name="AttachmentCount">The number of principals the policy is attached to.</param>
/// <param name="IsAttachable">Whether the policy may be attached to a principal.</param>
/// <param name="Description">The description of the policy, if any.</param>
/// <param name="CreateDate">When the policy was created, if known.</param>
/// <param name="UpdateDate">When the policy was last updated, if known.</param>
public sealed record IamPolicySummaryResponse(
    string PolicyName,
    string Arn,
    string PolicyId,
    string Path,
    string DefaultVersionId,
    int AttachmentCount,
    bool IsAttachable,
    string? Description,
    DateTimeOffset? CreateDate,
    DateTimeOffset? UpdateDate);

/// <summary>
/// The full detail of an IAM managed policy, including its default version document and the list of
/// available versions.
/// </summary>
/// <param name="PolicyName">The name of the policy.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the policy.</param>
/// <param name="PolicyId">The stable identifier assigned to the policy.</param>
/// <param name="Path">The path under which the policy is organised.</param>
/// <param name="DefaultVersionId">The identifier of the version that is currently the default.</param>
/// <param name="AttachmentCount">The number of principals the policy is attached to.</param>
/// <param name="IsAttachable">Whether the policy may be attached to a principal.</param>
/// <param name="Description">The description of the policy, if any.</param>
/// <param name="CreateDate">When the policy was created, if known.</param>
/// <param name="UpdateDate">When the policy was last updated, if known.</param>
/// <param name="DefaultVersionDocument">The JSON document of the default policy version.</param>
/// <param name="Versions">The versions of the policy.</param>
/// <param name="Tags">The key/value tags applied to the policy.</param>
public sealed record IamPolicyDetailResponse(
    string PolicyName,
    string Arn,
    string PolicyId,
    string Path,
    string DefaultVersionId,
    int AttachmentCount,
    bool IsAttachable,
    string? Description,
    DateTimeOffset? CreateDate,
    DateTimeOffset? UpdateDate,
    string DefaultVersionDocument,
    IReadOnlyList<IamPolicyVersionResponse> Versions,
    IReadOnlyList<IamTagResponse> Tags);

/// <summary>
/// A single version of an IAM managed policy.
/// </summary>
/// <param name="VersionId">The identifier of the version, such as v1.</param>
/// <param name="IsDefaultVersion">Whether this version is the default version of the policy.</param>
/// <param name="CreateDate">When the version was created, if known.</param>
public sealed record IamPolicyVersionResponse(
    string VersionId,
    bool IsDefaultVersion,
    DateTimeOffset? CreateDate);

/// <summary>
/// The versions of an IAM managed policy.
/// </summary>
/// <param name="Versions">The versions, ordered as returned by the backend.</param>
public sealed record IamPolicyVersionListResponse(IReadOnlyList<IamPolicyVersionResponse> Versions);

/// <summary>
/// A request to create a customer managed policy.
/// </summary>
/// <param name="PolicyName">The name of the policy to create.</param>
/// <param name="PolicyDocument">The JSON policy document for the initial version.</param>
/// <param name="Description">The optional description for the policy.</param>
/// <param name="Path">The optional path for the policy.</param>
public sealed record IamPolicyCreateRequest(
    string PolicyName,
    string PolicyDocument,
    string? Description,
    string? Path);

/// <summary>
/// A request to create a new version of an existing managed policy.
/// </summary>
/// <param name="PolicyArn">The Amazon Resource Name of the policy to add a version to.</param>
/// <param name="PolicyDocument">The JSON policy document for the new version.</param>
/// <param name="SetAsDefault">Whether the new version should become the default version.</param>
public sealed record IamPolicyVersionCreateRequest(
    string PolicyArn,
    string PolicyDocument,
    bool SetAsDefault);

/// <summary>
/// A request to set the default version of a managed policy.
/// </summary>
/// <param name="PolicyArn">The Amazon Resource Name of the policy.</param>
/// <param name="VersionId">The identifier of the version to make the default.</param>
public sealed record IamPolicyDefaultVersionRequest(string PolicyArn, string VersionId);

/// <summary>
/// The account-wide IAM entity counts and quotas reported by the backend.
/// </summary>
/// <param name="Entries">The summary entries keyed by name, such as Users or PoliciesQuota.</param>
public sealed record IamAccountSummaryResponse(IReadOnlyDictionary<string, int> Entries);

/// <summary>
/// The account password policy that governs IAM user passwords on the backend.
/// </summary>
/// <param name="MinimumPasswordLength">The minimum number of characters a password must contain.</param>
/// <param name="RequireSymbols">Whether passwords must contain at least one symbol.</param>
/// <param name="RequireNumbers">Whether passwords must contain at least one number.</param>
/// <param name="RequireUppercaseCharacters">Whether passwords must contain at least one uppercase letter.</param>
/// <param name="RequireLowercaseCharacters">Whether passwords must contain at least one lowercase letter.</param>
/// <param name="AllowUsersToChangePassword">Whether users may change their own password.</param>
/// <param name="ExpirePasswords">Whether passwords expire after <paramref name="MaxPasswordAge"/> days.</param>
/// <param name="MaxPasswordAge">The number of days before a password expires, if expiry is enabled.</param>
/// <param name="PasswordReusePrevention">The number of previous passwords that may not be reused, if set.</param>
/// <param name="HardExpiry">Whether users are prevented from setting a new password after expiry.</param>
public sealed record IamPasswordPolicyResponse(
    int MinimumPasswordLength,
    bool RequireSymbols,
    bool RequireNumbers,
    bool RequireUppercaseCharacters,
    bool RequireLowercaseCharacters,
    bool AllowUsersToChangePassword,
    bool ExpirePasswords,
    int? MaxPasswordAge,
    int? PasswordReusePrevention,
    bool HardExpiry);

/// <summary>
/// A request to create or replace the account password policy.
/// </summary>
/// <param name="MinimumPasswordLength">The minimum number of characters a password must contain.</param>
/// <param name="RequireSymbols">Whether passwords must contain at least one symbol.</param>
/// <param name="RequireNumbers">Whether passwords must contain at least one number.</param>
/// <param name="RequireUppercaseCharacters">Whether passwords must contain at least one uppercase letter.</param>
/// <param name="RequireLowercaseCharacters">Whether passwords must contain at least one lowercase letter.</param>
/// <param name="AllowUsersToChangePassword">Whether users may change their own password.</param>
/// <param name="MaxPasswordAge">The number of days before a password expires, or <see langword="null"/> for no expiry.</param>
/// <param name="PasswordReusePrevention">The number of previous passwords that may not be reused, or <see langword="null"/> for none.</param>
/// <param name="HardExpiry">Whether users are prevented from setting a new password after expiry.</param>
public sealed record IamPasswordPolicyRequest(
    int MinimumPasswordLength,
    bool RequireSymbols,
    bool RequireNumbers,
    bool RequireUppercaseCharacters,
    bool RequireLowercaseCharacters,
    bool AllowUsersToChangePassword,
    int? MaxPasswordAge,
    int? PasswordReusePrevention,
    bool HardExpiry);

/// <summary>
/// The account aliases configured on the backend.
/// </summary>
/// <param name="Aliases">The account aliases, ordered as returned by the backend.</param>
public sealed record IamAccountAliasListResponse(IReadOnlyList<string> Aliases);

/// <summary>
/// A request to create an account alias.
/// </summary>
/// <param name="AccountAlias">The account alias to create.</param>
public sealed record IamAccountAliasRequest(string AccountAlias);

/// <summary>
/// A request to add or update key/value tags on an IAM principal or managed policy.
/// </summary>
/// <param name="Tags">The tags to add or update.</param>
public sealed record IamTagsRequest(IReadOnlyList<IamTagRequest> Tags);

/// <summary>
/// A single key/value tag to apply to an IAM principal or managed policy.
/// </summary>
/// <param name="Key">The tag key.</param>
/// <param name="Value">The tag value.</param>
public sealed record IamTagRequest(string Key, string Value);

/// <summary>
/// A request to set the permissions boundary of an IAM principal to a managed policy.
/// </summary>
/// <param name="PermissionsBoundaryArn">The ARN of the managed policy to use as the boundary.</param>
public sealed record IamPermissionsBoundaryRequest(string PermissionsBoundaryArn);
