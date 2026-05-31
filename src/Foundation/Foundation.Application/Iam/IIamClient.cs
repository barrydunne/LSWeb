using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Iam;

namespace Foundation.Application.Iam;

/// <summary>
/// Provides access to AWS Identity and Access Management on the configured backend. This abstraction
/// starts with a lightweight reachability probe used by the IAM shell; user, group, role, and policy
/// operations are added by later tasks without changing the existing surface.
/// </summary>
public interface IIamClient
{
    /// <summary>
    /// Confirms that the IAM service on the configured backend is reachable.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result if IAM responded, or an error if the backend could not be reached.</returns>
    Task<Result> CheckConnectivityAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists the IAM users available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The users, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<IamUser>>> ListUsersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full detail of a single IAM user, including its groups, attached managed policies,
    /// inline policy names, and access keys with their most recent usage.
    /// </summary>
    /// <param name="userName">The name of the user to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The user detail, or an error if the user is missing or the backend could not be reached.</returns>
    Task<Result<IamUserDetail>> GetUserAsync(string userName, CancellationToken cancellationToken);

    /// <summary>
    /// Creates an IAM user with the supplied name and optional path.
    /// </summary>
    /// <param name="userName">The name of the user to create.</param>
    /// <param name="path">The optional path for the user, or <see langword="null"/> for the default path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the user could not be created.</returns>
    Task<Result> CreateUserAsync(string userName, string? path, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an IAM user after removing its access keys, inline policies, group memberships, and
    /// detaching its managed policies so the delete is not blocked by dependent resources.
    /// </summary>
    /// <param name="userName">The name of the user to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the user could not be deleted.</returns>
    Task<Result> DeleteUserAsync(string userName, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an IAM user to a group.
    /// </summary>
    /// <param name="userName">The name of the user to add.</param>
    /// <param name="groupName">The name of the group to add the user to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the membership could not be added.</returns>
    Task<Result> AddUserToGroupAsync(string userName, string groupName, CancellationToken cancellationToken);

    /// <summary>
    /// Removes an IAM user from a group.
    /// </summary>
    /// <param name="userName">The name of the user to remove.</param>
    /// <param name="groupName">The name of the group to remove the user from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the membership could not be removed.</returns>
    Task<Result> RemoveUserFromGroupAsync(string userName, string groupName, CancellationToken cancellationToken);

    /// <summary>
    /// Attaches a managed policy to an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user to attach the policy to.</param>
    /// <param name="policyArn">The ARN of the managed policy to attach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the policy could not be attached.</returns>
    Task<Result> AttachUserPolicyAsync(string userName, string policyArn, CancellationToken cancellationToken);

    /// <summary>
    /// Detaches a managed policy from an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user to detach the policy from.</param>
    /// <param name="policyArn">The ARN of the managed policy to detach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the policy could not be detached.</returns>
    Task<Result> DetachUserPolicyAsync(string userName, string policyArn, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or replaces an inline policy on an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user to put the inline policy on.</param>
    /// <param name="policyName">The name of the inline policy.</param>
    /// <param name="policyDocument">The JSON policy document.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the inline policy could not be written.</returns>
    Task<Result> PutUserInlinePolicyAsync(string userName, string policyName, string policyDocument, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an inline policy from an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user to delete the inline policy from.</param>
    /// <param name="policyName">The name of the inline policy to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the inline policy could not be deleted.</returns>
    Task<Result> DeleteUserInlinePolicyAsync(string userName, string policyName, CancellationToken cancellationToken);

    /// <summary>
    /// Creates an access key for an IAM user. The secret access key is only returned at creation time.
    /// </summary>
    /// <param name="userName">The name of the user to create the access key for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The new access key including its secret, or an error if it could not be created.</returns>
    Task<Result<IamAccessKeySecret>> CreateAccessKeyAsync(string userName, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the status of an IAM user's access key.
    /// </summary>
    /// <param name="userName">The name of the user that owns the access key.</param>
    /// <param name="accessKeyId">The identifier of the access key to update.</param>
    /// <param name="status">The new status, either <c>Active</c> or <c>Inactive</c>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the status could not be updated.</returns>
    Task<Result> UpdateAccessKeyStatusAsync(string userName, string accessKeyId, string status, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an IAM user's access key.
    /// </summary>
    /// <param name="userName">The name of the user that owns the access key.</param>
    /// <param name="accessKeyId">The identifier of the access key to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the access key could not be deleted.</returns>
    Task<Result> DeleteAccessKeyAsync(string userName, string accessKeyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the IAM groups available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The groups, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<IamGroup>>> ListGroupsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full detail of a single IAM group, including its members, attached managed policies,
    /// and inline policies with their documents.
    /// </summary>
    /// <param name="groupName">The name of the group to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The group detail, or an error if the group is missing or the backend could not be reached.</returns>
    Task<Result<IamGroupDetail>> GetGroupAsync(string groupName, CancellationToken cancellationToken);

    /// <summary>
    /// Creates an IAM group with the supplied name and optional path.
    /// </summary>
    /// <param name="groupName">The name of the group to create.</param>
    /// <param name="path">The optional path for the group, or <see langword="null"/> for the default path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the group could not be created.</returns>
    Task<Result> CreateGroupAsync(string groupName, string? path, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an IAM group after removing its members, detaching its managed policies, and deleting its
    /// inline policies so the delete is not blocked by dependent resources.
    /// </summary>
    /// <param name="groupName">The name of the group to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the group could not be deleted.</returns>
    Task<Result> DeleteGroupAsync(string groupName, CancellationToken cancellationToken);

    /// <summary>
    /// Attaches a managed policy to an IAM group.
    /// </summary>
    /// <param name="groupName">The name of the group to attach the policy to.</param>
    /// <param name="policyArn">The ARN of the managed policy to attach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the policy could not be attached.</returns>
    Task<Result> AttachGroupPolicyAsync(string groupName, string policyArn, CancellationToken cancellationToken);

    /// <summary>
    /// Detaches a managed policy from an IAM group.
    /// </summary>
    /// <param name="groupName">The name of the group to detach the policy from.</param>
    /// <param name="policyArn">The ARN of the managed policy to detach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the policy could not be detached.</returns>
    Task<Result> DetachGroupPolicyAsync(string groupName, string policyArn, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or replaces an inline policy on an IAM group.
    /// </summary>
    /// <param name="groupName">The name of the group to put the inline policy on.</param>
    /// <param name="policyName">The name of the inline policy.</param>
    /// <param name="policyDocument">The JSON policy document.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the inline policy could not be written.</returns>
    Task<Result> PutGroupInlinePolicyAsync(string groupName, string policyName, string policyDocument, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an inline policy from an IAM group.
    /// </summary>
    /// <param name="groupName">The name of the group to delete the inline policy from.</param>
    /// <param name="policyName">The name of the inline policy to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the inline policy could not be deleted.</returns>
    Task<Result> DeleteGroupInlinePolicyAsync(string groupName, string policyName, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the IAM roles available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The roles, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<IamRole>>> ListRolesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full detail of a single IAM role, including its trust policy, attached managed policies,
    /// and inline policies with their documents.
    /// </summary>
    /// <param name="roleName">The name of the role to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The role detail, or an error if the role is missing or the backend could not be reached.</returns>
    Task<Result<IamRoleDetail>> GetRoleAsync(string roleName, CancellationToken cancellationToken);

    /// <summary>
    /// Creates an IAM role with the supplied name, trust policy, and optional path, description, and session duration.
    /// </summary>
    /// <param name="roleName">The name of the role to create.</param>
    /// <param name="path">The optional path for the role, or <see langword="null"/> for the default path.</param>
    /// <param name="assumeRolePolicyDocument">The trust policy JSON document that controls who may assume the role.</param>
    /// <param name="description">The optional description for the role, or <see langword="null"/> for none.</param>
    /// <param name="maxSessionDuration">The optional maximum session duration in seconds, or <see langword="null"/> for the default.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the role could not be created.</returns>
    Task<Result> CreateRoleAsync(string roleName, string? path, string assumeRolePolicyDocument, string? description, int? maxSessionDuration, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the description and maximum session duration of an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to update.</param>
    /// <param name="description">The optional new description, or <see langword="null"/> to leave it unchanged.</param>
    /// <param name="maxSessionDuration">The optional new maximum session duration in seconds, or <see langword="null"/> to leave it unchanged.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the role could not be updated.</returns>
    Task<Result> UpdateRoleAsync(string roleName, string? description, int? maxSessionDuration, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an IAM role after detaching its managed policies and deleting its inline policies so the
    /// delete is not blocked by dependent resources.
    /// </summary>
    /// <param name="roleName">The name of the role to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the role could not be deleted.</returns>
    Task<Result> DeleteRoleAsync(string roleName, CancellationToken cancellationToken);

    /// <summary>
    /// Attaches a managed policy to an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to attach the policy to.</param>
    /// <param name="policyArn">The ARN of the managed policy to attach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the policy could not be attached.</returns>
    Task<Result> AttachRolePolicyAsync(string roleName, string policyArn, CancellationToken cancellationToken);

    /// <summary>
    /// Detaches a managed policy from an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to detach the policy from.</param>
    /// <param name="policyArn">The ARN of the managed policy to detach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the policy could not be detached.</returns>
    Task<Result> DetachRolePolicyAsync(string roleName, string policyArn, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or replaces an inline policy on an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to put the inline policy on.</param>
    /// <param name="policyName">The name of the inline policy.</param>
    /// <param name="policyDocument">The JSON policy document.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the inline policy could not be written.</returns>
    Task<Result> PutRoleInlinePolicyAsync(string roleName, string policyName, string policyDocument, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an inline policy from an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to delete the inline policy from.</param>
    /// <param name="policyName">The name of the inline policy to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the inline policy could not be deleted.</returns>
    Task<Result> DeleteRoleInlinePolicyAsync(string roleName, string policyName, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the trust policy that controls who may assume an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to update.</param>
    /// <param name="policyDocument">The new trust policy JSON document.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the trust policy could not be updated.</returns>
    Task<Result> UpdateRoleTrustPolicyAsync(string roleName, string policyDocument, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the IAM managed policies available on the configured backend.
    /// </summary>
    /// <param name="awsManaged">
    /// When <see langword="false"/>, only customer (local) managed policies are returned; when
    /// <see langword="true"/>, only AWS managed policies are returned for use in attach pickers.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The managed policies, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<IamPolicy>>> ListPoliciesAsync(bool awsManaged, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full detail of a single IAM managed policy, including its default version document and
    /// the full list of versions.
    /// </summary>
    /// <param name="policyArn">The ARN of the policy to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The policy detail, or an error if the policy is missing or the backend could not be reached.</returns>
    Task<Result<IamPolicyDetail>> GetPolicyAsync(string policyArn, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the versions of an IAM managed policy.
    /// </summary>
    /// <param name="policyArn">The ARN of the policy whose versions to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The policy versions, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<IamPolicyVersion>>> ListPolicyVersionsAsync(string policyArn, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a customer managed policy with the supplied name, document, and optional description and path.
    /// </summary>
    /// <param name="policyName">The name of the policy to create.</param>
    /// <param name="policyDocument">The JSON policy document for the initial version.</param>
    /// <param name="description">The optional description for the policy, or <see langword="null"/> for none.</param>
    /// <param name="path">The optional path for the policy, or <see langword="null"/> for the default path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the policy could not be created.</returns>
    Task<Result> CreatePolicyAsync(string policyName, string policyDocument, string? description, string? path, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new version of a customer managed policy, optionally promoting it to the default version.
    /// </summary>
    /// <param name="policyArn">The ARN of the policy to add a version to.</param>
    /// <param name="policyDocument">The JSON policy document for the new version.</param>
    /// <param name="setAsDefault">Whether the new version should become the policy's default version.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the version could not be created.</returns>
    Task<Result> CreatePolicyVersionAsync(string policyArn, string policyDocument, bool setAsDefault, CancellationToken cancellationToken);

    /// <summary>
    /// Promotes an existing version of a customer managed policy to be the default version.
    /// </summary>
    /// <param name="policyArn">The ARN of the policy to update.</param>
    /// <param name="versionId">The identifier of the version to make the default.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the default version could not be set.</returns>
    Task<Result> SetDefaultPolicyVersionAsync(string policyArn, string versionId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a non-default version of a customer managed policy.
    /// </summary>
    /// <param name="policyArn">The ARN of the policy whose version to delete.</param>
    /// <param name="versionId">The identifier of the version to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the version could not be deleted.</returns>
    Task<Result> DeletePolicyVersionAsync(string policyArn, string versionId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a customer managed policy. The policy must not be attached to any principals.
    /// </summary>
    /// <param name="policyArn">The ARN of the policy to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the policy could not be deleted.</returns>
    Task<Result> DeletePolicyAsync(string policyArn, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the account-wide IAM entity counts and quotas reported by the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The account summary, or an error if the backend could not be reached.</returns>
    Task<Result<IamAccountSummary>> GetAccountSummaryAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the account password policy, or <see langword="null"/> when no policy has been set.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The password policy, <see langword="null"/> if none is set, or an error if the backend could not be reached.</returns>
    Task<Result<IamPasswordPolicy?>> GetAccountPasswordPolicyAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists the account aliases configured on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The account aliases, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<string>>> ListAccountAliasesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates or replaces the account password policy.
    /// </summary>
    /// <param name="policy">The password policy to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the policy could not be updated.</returns>
    Task<Result> UpdateAccountPasswordPolicyAsync(IamPasswordPolicy policy, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the account password policy so that the backend default applies.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the policy could not be deleted.</returns>
    Task<Result> DeleteAccountPasswordPolicyAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates an account alias.
    /// </summary>
    /// <param name="accountAlias">The alias to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the alias could not be created.</returns>
    Task<Result> CreateAccountAliasAsync(string accountAlias, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an account alias.
    /// </summary>
    /// <param name="accountAlias">The alias to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the alias could not be deleted.</returns>
    Task<Result> DeleteAccountAliasAsync(string accountAlias, CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates key/value tags on an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user to tag.</param>
    /// <param name="tags">The tags to add or update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the tags could not be written.</returns>
    Task<Result> TagUserAsync(string userName, IReadOnlyList<IamTag> tags, CancellationToken cancellationToken);

    /// <summary>
    /// Removes tags from an IAM user by key.
    /// </summary>
    /// <param name="userName">The name of the user to untag.</param>
    /// <param name="tagKeys">The keys of the tags to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the tags could not be removed.</returns>
    Task<Result> UntagUserAsync(string userName, IReadOnlyList<string> tagKeys, CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates key/value tags on an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to tag.</param>
    /// <param name="tags">The tags to add or update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the tags could not be written.</returns>
    Task<Result> TagRoleAsync(string roleName, IReadOnlyList<IamTag> tags, CancellationToken cancellationToken);

    /// <summary>
    /// Removes tags from an IAM role by key.
    /// </summary>
    /// <param name="roleName">The name of the role to untag.</param>
    /// <param name="tagKeys">The keys of the tags to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the tags could not be removed.</returns>
    Task<Result> UntagRoleAsync(string roleName, IReadOnlyList<string> tagKeys, CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates key/value tags on an IAM managed policy.
    /// </summary>
    /// <param name="policyArn">The ARN of the policy to tag.</param>
    /// <param name="tags">The tags to add or update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the tags could not be written.</returns>
    Task<Result> TagPolicyAsync(string policyArn, IReadOnlyList<IamTag> tags, CancellationToken cancellationToken);

    /// <summary>
    /// Removes tags from an IAM managed policy by key.
    /// </summary>
    /// <param name="policyArn">The ARN of the policy to untag.</param>
    /// <param name="tagKeys">The keys of the tags to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the tags could not be removed.</returns>
    Task<Result> UntagPolicyAsync(string policyArn, IReadOnlyList<string> tagKeys, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the permissions boundary of an IAM user to the supplied managed policy.
    /// </summary>
    /// <param name="userName">The name of the user to set the boundary on.</param>
    /// <param name="permissionsBoundaryArn">The ARN of the managed policy to use as the boundary.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the boundary could not be set.</returns>
    Task<Result> PutUserPermissionsBoundaryAsync(string userName, string permissionsBoundaryArn, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the permissions boundary from an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user to remove the boundary from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the boundary could not be removed.</returns>
    Task<Result> DeleteUserPermissionsBoundaryAsync(string userName, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the permissions boundary of an IAM role to the supplied managed policy.
    /// </summary>
    /// <param name="roleName">The name of the role to set the boundary on.</param>
    /// <param name="permissionsBoundaryArn">The ARN of the managed policy to use as the boundary.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the boundary could not be set.</returns>
    Task<Result> PutRolePermissionsBoundaryAsync(string roleName, string permissionsBoundaryArn, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the permissions boundary from an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to remove the boundary from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the boundary could not be removed.</returns>
    Task<Result> DeleteRolePermissionsBoundaryAsync(string roleName, CancellationToken cancellationToken);
}
