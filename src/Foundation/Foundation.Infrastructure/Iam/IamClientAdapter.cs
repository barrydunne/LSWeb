using System.Diagnostics.CodeAnalysis;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Domain.Iam;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.Iam;

/// <summary>
/// Reads IAM through the resilient AWS gateway so the same code works against LocalStack or real AWS.
/// All access flows through <see cref="IAwsGateway"/>, which records capability and converts failures
/// into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class IamClientAdapter : IIamClient
{
    private const string ServiceKey = "iam";

    private readonly IAwsGateway _gateway;

    public IamClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public async Task<Result> CheckConnectivityAsync(CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.ListAccountAliasesAsync(new ListAccountAliasesRequest { MaxItems = 1 }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<IamUser>>> ListUsersAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IReadOnlyList<IamUser>>(
            ServiceKey,
            async (client, token) =>
            {
                var users = new List<IamUser>();
                string? marker = null;

                do
                {
                    var response = await client.ListUsersAsync(
                        new ListUsersRequest { Marker = marker },
                        token);

                    foreach (var user in response.Users ?? [])
                        users.Add(ToUser(user));

                    marker = response.IsTruncated == true ? response.Marker : null;
                }
                while (!string.IsNullOrEmpty(marker));

                return users;
            },
            cancellationToken);

    public Task<Result<IamUserDetail>> GetUserAsync(string userName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IamUserDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var user = await client.GetUserAsync(new GetUserRequest { UserName = userName }, token);
                var groups = await GetGroupsAsync(client, userName, token);
                var attachedPolicies = await GetAttachedPoliciesAsync(client, userName, token);
                var inlinePolicyNames = await GetInlinePolicyNamesAsync(client, userName, token);
                var accessKeys = await GetAccessKeysAsync(client, userName, token);

                return new IamUserDetail(
                    user.User.UserName ?? userName,
                    user.User.Arn ?? string.Empty,
                    user.User.UserId ?? string.Empty,
                    user.User.Path ?? "/",
                    ToOffset(user.User.CreateDate),
                    groups,
                    attachedPolicies,
                    inlinePolicyNames,
                    accessKeys,
                    ToTags(user.User.Tags),
                    user.User.PermissionsBoundary?.PermissionsBoundaryArn);
            },
            cancellationToken);

    public async Task<Result> CreateUserAsync(string userName, string? path, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateUserAsync(
                    new CreateUserRequest { UserName = userName, Path = string.IsNullOrEmpty(path) ? null : path },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteUserAsync(string userName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await DeleteAccessKeysAsync(client, userName, token);
                await DeleteInlinePoliciesAsync(client, userName, token);
                await DetachManagedPoliciesAsync(client, userName, token);
                await RemoveFromGroupsAsync(client, userName, token);
                await client.DeleteUserAsync(new DeleteUserRequest { UserName = userName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> AddUserToGroupAsync(string userName, string groupName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.AddUserToGroupAsync(
                    new AddUserToGroupRequest { UserName = userName, GroupName = groupName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> RemoveUserFromGroupAsync(string userName, string groupName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.RemoveUserFromGroupAsync(
                    new RemoveUserFromGroupRequest { UserName = userName, GroupName = groupName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> AttachUserPolicyAsync(string userName, string policyArn, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.AttachUserPolicyAsync(
                    new AttachUserPolicyRequest { UserName = userName, PolicyArn = policyArn },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DetachUserPolicyAsync(string userName, string policyArn, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DetachUserPolicyAsync(
                    new DetachUserPolicyRequest { UserName = userName, PolicyArn = policyArn },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> PutUserInlinePolicyAsync(
        string userName, string policyName, string policyDocument, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutUserPolicyAsync(
                    new PutUserPolicyRequest
                    {
                        UserName = userName,
                        PolicyName = policyName,
                        PolicyDocument = policyDocument,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteUserInlinePolicyAsync(string userName, string policyName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteUserPolicyAsync(
                    new DeleteUserPolicyRequest { UserName = userName, PolicyName = policyName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IamAccessKeySecret>> CreateAccessKeyAsync(string userName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IamAccessKeySecret>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.CreateAccessKeyAsync(
                    new CreateAccessKeyRequest { UserName = userName },
                    token);

                var key = response.AccessKey;
                return new IamAccessKeySecret(
                    key.AccessKeyId ?? string.Empty,
                    key.SecretAccessKey ?? string.Empty,
                    key.Status?.Value ?? string.Empty,
                    ToOffset(key.CreateDate));
            },
            cancellationToken);

    public async Task<Result> UpdateAccessKeyStatusAsync(
        string userName, string accessKeyId, string status, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UpdateAccessKeyAsync(
                    new UpdateAccessKeyRequest
                    {
                        UserName = userName,
                        AccessKeyId = accessKeyId,
                        Status = StatusType.FindValue(status),
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteAccessKeyAsync(string userName, string accessKeyId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteAccessKeyAsync(
                    new DeleteAccessKeyRequest { UserName = userName, AccessKeyId = accessKeyId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<IamGroup>>> ListGroupsAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IReadOnlyList<IamGroup>>(
            ServiceKey,
            async (client, token) =>
            {
                var groups = new List<IamGroup>();
                string? marker = null;

                do
                {
                    var response = await client.ListGroupsAsync(
                        new ListGroupsRequest { Marker = marker },
                        token);

                    foreach (var group in response.Groups ?? [])
                        groups.Add(ToGroup(group));

                    marker = response.IsTruncated == true ? response.Marker : null;
                }
                while (!string.IsNullOrEmpty(marker));

                return groups;
            },
            cancellationToken);

    public Task<Result<IamGroupDetail>> GetGroupAsync(string groupName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IamGroupDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var (group, members) = await GetGroupWithMembersAsync(client, groupName, token);
                var attachedPolicies = await GetAttachedGroupPoliciesAsync(client, groupName, token);
                var inlinePolicies = await GetInlineGroupPoliciesAsync(client, groupName, token);

                return new IamGroupDetail(
                    group.GroupName ?? groupName,
                    group.Arn ?? string.Empty,
                    group.GroupId ?? string.Empty,
                    group.Path ?? "/",
                    ToOffset(group.CreateDate),
                    members,
                    attachedPolicies,
                    inlinePolicies);
            },
            cancellationToken);

    public async Task<Result> CreateGroupAsync(string groupName, string? path, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateGroupAsync(
                    new CreateGroupRequest { GroupName = groupName, Path = string.IsNullOrEmpty(path) ? null : path },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteGroupAsync(string groupName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await RemoveGroupMembersAsync(client, groupName, token);
                await DeleteGroupInlinePoliciesAsync(client, groupName, token);
                await DetachGroupManagedPoliciesAsync(client, groupName, token);
                await client.DeleteGroupAsync(new DeleteGroupRequest { GroupName = groupName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> AttachGroupPolicyAsync(string groupName, string policyArn, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.AttachGroupPolicyAsync(
                    new AttachGroupPolicyRequest { GroupName = groupName, PolicyArn = policyArn },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DetachGroupPolicyAsync(string groupName, string policyArn, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DetachGroupPolicyAsync(
                    new DetachGroupPolicyRequest { GroupName = groupName, PolicyArn = policyArn },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> PutGroupInlinePolicyAsync(
        string groupName, string policyName, string policyDocument, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutGroupPolicyAsync(
                    new PutGroupPolicyRequest
                    {
                        GroupName = groupName,
                        PolicyName = policyName,
                        PolicyDocument = policyDocument,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteGroupInlinePolicyAsync(string groupName, string policyName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteGroupPolicyAsync(
                    new DeleteGroupPolicyRequest { GroupName = groupName, PolicyName = policyName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static async Task<(Group Group, IReadOnlyList<string> Members)> GetGroupWithMembersAsync(
        AmazonIdentityManagementServiceClient client, string groupName, CancellationToken token)
    {
        var members = new List<string>();
        Group? group = null;
        string? marker = null;

        do
        {
            var response = await client.GetGroupAsync(
                new GetGroupRequest { GroupName = groupName, Marker = marker },
                token);

            group ??= response.Group;

            foreach (var user in response.Users ?? [])
                members.Add(user.UserName ?? string.Empty);

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));

        return (group ?? new Group { GroupName = groupName }, members);
    }

    private static async Task<IReadOnlyList<IamAttachedPolicy>> GetAttachedGroupPoliciesAsync(
        AmazonIdentityManagementServiceClient client, string groupName, CancellationToken token)
    {
        var policies = new List<IamAttachedPolicy>();
        string? marker = null;

        do
        {
            var response = await client.ListAttachedGroupPoliciesAsync(
                new ListAttachedGroupPoliciesRequest { GroupName = groupName, Marker = marker },
                token);

            foreach (var policy in response.AttachedPolicies ?? [])
                policies.Add(new IamAttachedPolicy(policy.PolicyName ?? string.Empty, policy.PolicyArn ?? string.Empty));

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));

        return policies;
    }

    private static async Task<IReadOnlyList<IamInlinePolicy>> GetInlineGroupPoliciesAsync(
        AmazonIdentityManagementServiceClient client, string groupName, CancellationToken token)
    {
        var policies = new List<IamInlinePolicy>();
        string? marker = null;

        do
        {
            var response = await client.ListGroupPoliciesAsync(
                new ListGroupPoliciesRequest { GroupName = groupName, Marker = marker },
                token);

            foreach (var policyName in response.PolicyNames ?? [])
            {
                var document = await client.GetGroupPolicyAsync(
                    new GetGroupPolicyRequest { GroupName = groupName, PolicyName = policyName },
                    token);

                policies.Add(new IamInlinePolicy(
                    document.PolicyName ?? policyName,
                    Uri.UnescapeDataString(document.PolicyDocument ?? string.Empty)));
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));

        return policies;
    }

    private static async Task RemoveGroupMembersAsync(
        AmazonIdentityManagementServiceClient client, string groupName, CancellationToken token)
    {
        string? marker = null;

        do
        {
            var response = await client.GetGroupAsync(
                new GetGroupRequest { GroupName = groupName, Marker = marker },
                token);

            foreach (var user in response.Users ?? [])
            {
                await client.RemoveUserFromGroupAsync(
                    new RemoveUserFromGroupRequest { UserName = user.UserName, GroupName = groupName },
                    token);
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));
    }

    private static async Task DeleteGroupInlinePoliciesAsync(
        AmazonIdentityManagementServiceClient client, string groupName, CancellationToken token)
    {
        string? marker = null;

        do
        {
            var response = await client.ListGroupPoliciesAsync(
                new ListGroupPoliciesRequest { GroupName = groupName, Marker = marker },
                token);

            foreach (var policyName in response.PolicyNames ?? [])
            {
                await client.DeleteGroupPolicyAsync(
                    new DeleteGroupPolicyRequest { GroupName = groupName, PolicyName = policyName },
                    token);
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));
    }

    private static async Task DetachGroupManagedPoliciesAsync(
        AmazonIdentityManagementServiceClient client, string groupName, CancellationToken token)
    {
        string? marker = null;

        do
        {
            var response = await client.ListAttachedGroupPoliciesAsync(
                new ListAttachedGroupPoliciesRequest { GroupName = groupName, Marker = marker },
                token);

            foreach (var policy in response.AttachedPolicies ?? [])
            {
                await client.DetachGroupPolicyAsync(
                    new DetachGroupPolicyRequest { GroupName = groupName, PolicyArn = policy.PolicyArn },
                    token);
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));
    }

    private static async Task DeleteAccessKeysAsync(
        AmazonIdentityManagementServiceClient client, string userName, CancellationToken token)
    {
        string? marker = null;

        do
        {
            var response = await client.ListAccessKeysAsync(
                new ListAccessKeysRequest { UserName = userName, Marker = marker },
                token);

            foreach (var metadata in response.AccessKeyMetadata ?? [])
            {
                await client.DeleteAccessKeyAsync(
                    new DeleteAccessKeyRequest { UserName = userName, AccessKeyId = metadata.AccessKeyId },
                    token);
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));
    }

    private static async Task DeleteInlinePoliciesAsync(
        AmazonIdentityManagementServiceClient client, string userName, CancellationToken token)
    {
        string? marker = null;

        do
        {
            var response = await client.ListUserPoliciesAsync(
                new ListUserPoliciesRequest { UserName = userName, Marker = marker },
                token);

            foreach (var policyName in response.PolicyNames ?? [])
            {
                await client.DeleteUserPolicyAsync(
                    new DeleteUserPolicyRequest { UserName = userName, PolicyName = policyName },
                    token);
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));
    }

    private static async Task DetachManagedPoliciesAsync(
        AmazonIdentityManagementServiceClient client, string userName, CancellationToken token)
    {
        string? marker = null;

        do
        {
            var response = await client.ListAttachedUserPoliciesAsync(
                new ListAttachedUserPoliciesRequest { UserName = userName, Marker = marker },
                token);

            foreach (var policy in response.AttachedPolicies ?? [])
            {
                await client.DetachUserPolicyAsync(
                    new DetachUserPolicyRequest { UserName = userName, PolicyArn = policy.PolicyArn },
                    token);
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));
    }

    private static async Task RemoveFromGroupsAsync(
        AmazonIdentityManagementServiceClient client, string userName, CancellationToken token)
    {
        string? marker = null;

        do
        {
            var response = await client.ListGroupsForUserAsync(
                new ListGroupsForUserRequest { UserName = userName, Marker = marker },
                token);

            foreach (var group in response.Groups ?? [])
            {
                await client.RemoveUserFromGroupAsync(
                    new RemoveUserFromGroupRequest { UserName = userName, GroupName = group.GroupName },
                    token);
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));
    }

    private static async Task<IReadOnlyList<string>> GetGroupsAsync(
        AmazonIdentityManagementServiceClient client, string userName, CancellationToken token)
    {
        var groups = new List<string>();
        string? marker = null;

        do
        {
            var response = await client.ListGroupsForUserAsync(
                new ListGroupsForUserRequest { UserName = userName, Marker = marker },
                token);

            foreach (var group in response.Groups ?? [])
                groups.Add(group.GroupName ?? string.Empty);

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));

        return groups;
    }

    private static async Task<IReadOnlyList<IamAttachedPolicy>> GetAttachedPoliciesAsync(
        AmazonIdentityManagementServiceClient client, string userName, CancellationToken token)
    {
        var policies = new List<IamAttachedPolicy>();
        string? marker = null;

        do
        {
            var response = await client.ListAttachedUserPoliciesAsync(
                new ListAttachedUserPoliciesRequest { UserName = userName, Marker = marker },
                token);

            foreach (var policy in response.AttachedPolicies ?? [])
                policies.Add(new IamAttachedPolicy(policy.PolicyName ?? string.Empty, policy.PolicyArn ?? string.Empty));

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));

        return policies;
    }

    private static async Task<IReadOnlyList<string>> GetInlinePolicyNamesAsync(
        AmazonIdentityManagementServiceClient client, string userName, CancellationToken token)
    {
        var names = new List<string>();
        string? marker = null;

        do
        {
            var response = await client.ListUserPoliciesAsync(
                new ListUserPoliciesRequest { UserName = userName, Marker = marker },
                token);

            names.AddRange(response.PolicyNames ?? []);

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));

        return names;
    }

    private static async Task<IReadOnlyList<IamAccessKey>> GetAccessKeysAsync(
        AmazonIdentityManagementServiceClient client, string userName, CancellationToken token)
    {
        var keys = new List<IamAccessKey>();
        string? marker = null;

        do
        {
            var response = await client.ListAccessKeysAsync(
                new ListAccessKeysRequest { UserName = userName, Marker = marker },
                token);

            foreach (var metadata in response.AccessKeyMetadata ?? [])
            {
                var lastUsed = await client.GetAccessKeyLastUsedAsync(
                    new GetAccessKeyLastUsedRequest { AccessKeyId = metadata.AccessKeyId },
                    token);

                keys.Add(new IamAccessKey(
                    metadata.AccessKeyId ?? string.Empty,
                    metadata.Status?.Value ?? string.Empty,
                    ToOffset(metadata.CreateDate),
                    ToOffset(lastUsed.AccessKeyLastUsed?.LastUsedDate),
                    NormaliseLastUsed(lastUsed.AccessKeyLastUsed?.ServiceName),
                    NormaliseLastUsed(lastUsed.AccessKeyLastUsed?.Region)));
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));

        return keys;
    }

    public Task<Result<IReadOnlyList<IamRole>>> ListRolesAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IReadOnlyList<IamRole>>(
            ServiceKey,
            async (client, token) =>
            {
                var roles = new List<IamRole>();
                string? marker = null;

                do
                {
                    var response = await client.ListRolesAsync(
                        new ListRolesRequest { Marker = marker },
                        token);

                    foreach (var role in response.Roles ?? [])
                        roles.Add(ToRole(role));

                    marker = response.IsTruncated == true ? response.Marker : null;
                }
                while (!string.IsNullOrEmpty(marker));

                return roles;
            },
            cancellationToken);

    public Task<Result<IamRoleDetail>> GetRoleAsync(string roleName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IamRoleDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetRoleAsync(new GetRoleRequest { RoleName = roleName }, token);
                var role = response.Role ?? new Role { RoleName = roleName };
                var attachedPolicies = await GetAttachedRolePoliciesAsync(client, roleName, token);
                var inlinePolicies = await GetInlineRolePoliciesAsync(client, roleName, token);

                return new IamRoleDetail(
                    role.RoleName ?? roleName,
                    role.Arn ?? string.Empty,
                    role.RoleId ?? string.Empty,
                    role.Path ?? "/",
                    ToOffset(role.CreateDate),
                    string.IsNullOrEmpty(role.Description) ? null : role.Description,
                    role.MaxSessionDuration,
                    Uri.UnescapeDataString(role.AssumeRolePolicyDocument ?? string.Empty),
                    attachedPolicies,
                    inlinePolicies,
                    ToTags(role.Tags),
                    role.PermissionsBoundary?.PermissionsBoundaryArn);
            },
            cancellationToken);

    public async Task<Result> CreateRoleAsync(
        string roleName, string? path, string assumeRolePolicyDocument, string? description, int? maxSessionDuration, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateRoleAsync(
                    new CreateRoleRequest
                    {
                        RoleName = roleName,
                        Path = string.IsNullOrEmpty(path) ? null : path,
                        AssumeRolePolicyDocument = assumeRolePolicyDocument,
                        Description = string.IsNullOrEmpty(description) ? null : description,
                        MaxSessionDuration = maxSessionDuration,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> UpdateRoleAsync(
        string roleName, string? description, int? maxSessionDuration, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UpdateRoleAsync(
                    new UpdateRoleRequest
                    {
                        RoleName = roleName,
                        Description = string.IsNullOrEmpty(description) ? null : description,
                        MaxSessionDuration = maxSessionDuration,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await DeleteRoleInlinePoliciesAsync(client, roleName, token);
                await DetachRoleManagedPoliciesAsync(client, roleName, token);
                await client.DeleteRoleAsync(new DeleteRoleRequest { RoleName = roleName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> AttachRolePolicyAsync(string roleName, string policyArn, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.AttachRolePolicyAsync(
                    new AttachRolePolicyRequest { RoleName = roleName, PolicyArn = policyArn },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DetachRolePolicyAsync(string roleName, string policyArn, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DetachRolePolicyAsync(
                    new DetachRolePolicyRequest { RoleName = roleName, PolicyArn = policyArn },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> PutRoleInlinePolicyAsync(
        string roleName, string policyName, string policyDocument, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutRolePolicyAsync(
                    new PutRolePolicyRequest
                    {
                        RoleName = roleName,
                        PolicyName = policyName,
                        PolicyDocument = policyDocument,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteRoleInlinePolicyAsync(string roleName, string policyName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteRolePolicyAsync(
                    new DeleteRolePolicyRequest { RoleName = roleName, PolicyName = policyName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> UpdateRoleTrustPolicyAsync(string roleName, string policyDocument, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UpdateAssumeRolePolicyAsync(
                    new UpdateAssumeRolePolicyRequest { RoleName = roleName, PolicyDocument = policyDocument },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<IamPolicy>>> ListPoliciesAsync(bool awsManaged, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IReadOnlyList<IamPolicy>>(
            ServiceKey,
            async (client, token) =>
            {
                var policies = new List<IamPolicy>();
                string? marker = null;

                do
                {
                    var response = await client.ListPoliciesAsync(
                        new ListPoliciesRequest
                        {
                            Scope = awsManaged ? PolicyScopeType.AWS : PolicyScopeType.Local,
                            Marker = marker,
                        },
                        token);

                    foreach (var policy in response.Policies ?? [])
                        policies.Add(ToPolicy(policy));

                    marker = response.IsTruncated == true ? response.Marker : null;
                }
                while (!string.IsNullOrEmpty(marker));

                return policies;
            },
            cancellationToken);

    public Task<Result<IamPolicyDetail>> GetPolicyAsync(string policyArn, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IamPolicyDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetPolicyAsync(new GetPolicyRequest { PolicyArn = policyArn }, token);
                var policy = response.Policy ?? new ManagedPolicy { Arn = policyArn };
                var defaultVersionId = policy.DefaultVersionId ?? string.Empty;

                var document = string.Empty;
                if (!string.IsNullOrEmpty(defaultVersionId))
                {
                    var version = await client.GetPolicyVersionAsync(
                        new GetPolicyVersionRequest { PolicyArn = policyArn, VersionId = defaultVersionId },
                        token);
                    document = Uri.UnescapeDataString(version.PolicyVersion?.Document ?? string.Empty);
                }

                var versions = await GetPolicyVersionsAsync(client, policyArn, token);

                return new IamPolicyDetail(
                    policy.PolicyName ?? string.Empty,
                    policy.Arn ?? policyArn,
                    policy.PolicyId ?? string.Empty,
                    policy.Path ?? "/",
                    defaultVersionId,
                    policy.AttachmentCount ?? 0,
                    policy.IsAttachable ?? false,
                    string.IsNullOrEmpty(policy.Description) ? null : policy.Description,
                    ToOffset(policy.CreateDate),
                    ToOffset(policy.UpdateDate),
                    document,
                    versions,
                    ToTags(policy.Tags));
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<IamPolicyVersion>>> ListPolicyVersionsAsync(string policyArn, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IReadOnlyList<IamPolicyVersion>>(
            ServiceKey,
            async (client, token) => await GetPolicyVersionsAsync(client, policyArn, token),
            cancellationToken);

    public async Task<Result> CreatePolicyAsync(
        string policyName, string policyDocument, string? description, string? path, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreatePolicyAsync(
                    new CreatePolicyRequest
                    {
                        PolicyName = policyName,
                        PolicyDocument = policyDocument,
                        Description = string.IsNullOrEmpty(description) ? null : description,
                        Path = string.IsNullOrEmpty(path) ? null : path,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> CreatePolicyVersionAsync(
        string policyArn, string policyDocument, bool setAsDefault, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreatePolicyVersionAsync(
                    new CreatePolicyVersionRequest
                    {
                        PolicyArn = policyArn,
                        PolicyDocument = policyDocument,
                        SetAsDefault = setAsDefault,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> SetDefaultPolicyVersionAsync(string policyArn, string versionId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.SetDefaultPolicyVersionAsync(
                    new SetDefaultPolicyVersionRequest { PolicyArn = policyArn, VersionId = versionId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeletePolicyVersionAsync(string policyArn, string versionId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeletePolicyVersionAsync(
                    new DeletePolicyVersionRequest { PolicyArn = policyArn, VersionId = versionId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeletePolicyAsync(string policyArn, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeletePolicyAsync(new DeletePolicyRequest { PolicyArn = policyArn }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IamAccountSummary>> GetAccountSummaryAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IamAccountSummary>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetAccountSummaryAsync(new GetAccountSummaryRequest(), token);
                var entries = (response.SummaryMap ?? [])
                    .OrderBy(_ => _.Key, StringComparer.Ordinal)
                    .ToDictionary(_ => _.Key, _ => _.Value, StringComparer.Ordinal);
                return new IamAccountSummary(entries);
            },
            cancellationToken);

    public Task<Result<IamPasswordPolicy?>> GetAccountPasswordPolicyAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IamPasswordPolicy?>(
            ServiceKey,
            async (client, token) =>
            {
                try
                {
                    var response = await client.GetAccountPasswordPolicyAsync(
                        new GetAccountPasswordPolicyRequest(), token);
                    return ToPasswordPolicy(response.PasswordPolicy);
                }
                catch (NoSuchEntityException)
                {
                    return null;
                }
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<string>>> ListAccountAliasesAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, IReadOnlyList<string>>(
            ServiceKey,
            async (client, token) =>
            {
                var aliases = new List<string>();
                string? marker = null;

                do
                {
                    var response = await client.ListAccountAliasesAsync(
                        new ListAccountAliasesRequest { Marker = marker },
                        token);

                    aliases.AddRange(response.AccountAliases ?? []);

                    marker = response.IsTruncated == true ? response.Marker : null;
                }
                while (!string.IsNullOrEmpty(marker));

                return aliases;
            },
            cancellationToken);

    public async Task<Result> UpdateAccountPasswordPolicyAsync(IamPasswordPolicy policy, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UpdateAccountPasswordPolicyAsync(
                    new UpdateAccountPasswordPolicyRequest
                    {
                        MinimumPasswordLength = policy.MinimumPasswordLength,
                        RequireSymbols = policy.RequireSymbols,
                        RequireNumbers = policy.RequireNumbers,
                        RequireUppercaseCharacters = policy.RequireUppercaseCharacters,
                        RequireLowercaseCharacters = policy.RequireLowercaseCharacters,
                        AllowUsersToChangePassword = policy.AllowUsersToChangePassword,
                        MaxPasswordAge = policy.MaxPasswordAge,
                        PasswordReusePrevention = policy.PasswordReusePrevention,
                        HardExpiry = policy.HardExpiry,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteAccountPasswordPolicyAsync(CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteAccountPasswordPolicyAsync(new DeleteAccountPasswordPolicyRequest(), token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> CreateAccountAliasAsync(string accountAlias, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateAccountAliasAsync(
                    new CreateAccountAliasRequest { AccountAlias = accountAlias },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteAccountAliasAsync(string accountAlias, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteAccountAliasAsync(
                    new DeleteAccountAliasRequest { AccountAlias = accountAlias },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> TagUserAsync(string userName, IReadOnlyList<IamTag> tags, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.TagUserAsync(
                    new TagUserRequest { UserName = userName, Tags = ToSdkTags(tags) },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> UntagUserAsync(string userName, IReadOnlyList<string> tagKeys, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UntagUserAsync(
                    new UntagUserRequest { UserName = userName, TagKeys = [.. tagKeys] },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> TagRoleAsync(string roleName, IReadOnlyList<IamTag> tags, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.TagRoleAsync(
                    new TagRoleRequest { RoleName = roleName, Tags = ToSdkTags(tags) },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> UntagRoleAsync(string roleName, IReadOnlyList<string> tagKeys, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UntagRoleAsync(
                    new UntagRoleRequest { RoleName = roleName, TagKeys = [.. tagKeys] },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> TagPolicyAsync(string policyArn, IReadOnlyList<IamTag> tags, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.TagPolicyAsync(
                    new TagPolicyRequest { PolicyArn = policyArn, Tags = ToSdkTags(tags) },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> UntagPolicyAsync(string policyArn, IReadOnlyList<string> tagKeys, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UntagPolicyAsync(
                    new UntagPolicyRequest { PolicyArn = policyArn, TagKeys = [.. tagKeys] },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> PutUserPermissionsBoundaryAsync(string userName, string permissionsBoundaryArn, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutUserPermissionsBoundaryAsync(
                    new PutUserPermissionsBoundaryRequest { UserName = userName, PermissionsBoundary = permissionsBoundaryArn },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteUserPermissionsBoundaryAsync(string userName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteUserPermissionsBoundaryAsync(
                    new DeleteUserPermissionsBoundaryRequest { UserName = userName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> PutRolePermissionsBoundaryAsync(string roleName, string permissionsBoundaryArn, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutRolePermissionsBoundaryAsync(
                    new PutRolePermissionsBoundaryRequest { RoleName = roleName, PermissionsBoundary = permissionsBoundaryArn },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteRolePermissionsBoundaryAsync(string roleName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonIdentityManagementServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteRolePermissionsBoundaryAsync(
                    new DeleteRolePermissionsBoundaryRequest { RoleName = roleName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static List<Tag> ToSdkTags(IReadOnlyList<IamTag> tags)
        => [.. tags.Select(tag => new Tag { Key = tag.Key, Value = tag.Value })];

    private static IReadOnlyList<IamTag> ToTags(List<Tag>? tags)
        => tags is null
            ? []
            : [.. tags.Select(tag => new IamTag(tag.Key ?? string.Empty, tag.Value ?? string.Empty))];

    private static IamPasswordPolicy ToPasswordPolicy(PasswordPolicy policy)
        => new(
            policy.MinimumPasswordLength ?? 0,
            policy.RequireSymbols ?? false,
            policy.RequireNumbers ?? false,
            policy.RequireUppercaseCharacters ?? false,
            policy.RequireLowercaseCharacters ?? false,
            policy.AllowUsersToChangePassword ?? false,
            policy.ExpirePasswords ?? false,
            policy.MaxPasswordAge,
            policy.PasswordReusePrevention,
            policy.HardExpiry ?? false);

    private static async Task<IReadOnlyList<IamPolicyVersion>> GetPolicyVersionsAsync(
        AmazonIdentityManagementServiceClient client, string policyArn, CancellationToken token)
    {
        var versions = new List<IamPolicyVersion>();
        string? marker = null;

        do
        {
            var response = await client.ListPolicyVersionsAsync(
                new ListPolicyVersionsRequest { PolicyArn = policyArn, Marker = marker },
                token);

            foreach (var version in response.Versions ?? [])
            {
                versions.Add(new IamPolicyVersion(
                    version.VersionId ?? string.Empty,
                    version.IsDefaultVersion ?? false,
                    ToOffset(version.CreateDate)));
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));

        return versions;
    }

    private static IamPolicy ToPolicy(ManagedPolicy policy)
        => new(
            policy.PolicyName ?? string.Empty,
            policy.Arn ?? string.Empty,
            policy.PolicyId ?? string.Empty,
            policy.Path ?? "/",
            policy.DefaultVersionId ?? string.Empty,
            policy.AttachmentCount ?? 0,
            policy.IsAttachable ?? false,
            string.IsNullOrEmpty(policy.Description) ? null : policy.Description,
            ToOffset(policy.CreateDate),
            ToOffset(policy.UpdateDate));

    private static async Task<IReadOnlyList<IamAttachedPolicy>> GetAttachedRolePoliciesAsync(
        AmazonIdentityManagementServiceClient client, string roleName, CancellationToken token)
    {
        var policies = new List<IamAttachedPolicy>();
        string? marker = null;

        do
        {
            var response = await client.ListAttachedRolePoliciesAsync(
                new ListAttachedRolePoliciesRequest { RoleName = roleName, Marker = marker },
                token);

            foreach (var policy in response.AttachedPolicies ?? [])
                policies.Add(new IamAttachedPolicy(policy.PolicyName ?? string.Empty, policy.PolicyArn ?? string.Empty));

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));

        return policies;
    }

    private static async Task<IReadOnlyList<IamInlinePolicy>> GetInlineRolePoliciesAsync(
        AmazonIdentityManagementServiceClient client, string roleName, CancellationToken token)
    {
        var policies = new List<IamInlinePolicy>();
        string? marker = null;

        do
        {
            var response = await client.ListRolePoliciesAsync(
                new ListRolePoliciesRequest { RoleName = roleName, Marker = marker },
                token);

            foreach (var policyName in response.PolicyNames ?? [])
            {
                var document = await client.GetRolePolicyAsync(
                    new GetRolePolicyRequest { RoleName = roleName, PolicyName = policyName },
                    token);

                policies.Add(new IamInlinePolicy(
                    document.PolicyName ?? policyName,
                    Uri.UnescapeDataString(document.PolicyDocument ?? string.Empty)));
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));

        return policies;
    }

    private static async Task DeleteRoleInlinePoliciesAsync(
        AmazonIdentityManagementServiceClient client, string roleName, CancellationToken token)
    {
        string? marker = null;

        do
        {
            var response = await client.ListRolePoliciesAsync(
                new ListRolePoliciesRequest { RoleName = roleName, Marker = marker },
                token);

            foreach (var policyName in response.PolicyNames ?? [])
            {
                await client.DeleteRolePolicyAsync(
                    new DeleteRolePolicyRequest { RoleName = roleName, PolicyName = policyName },
                    token);
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));
    }

    private static async Task DetachRoleManagedPoliciesAsync(
        AmazonIdentityManagementServiceClient client, string roleName, CancellationToken token)
    {
        string? marker = null;

        do
        {
            var response = await client.ListAttachedRolePoliciesAsync(
                new ListAttachedRolePoliciesRequest { RoleName = roleName, Marker = marker },
                token);

            foreach (var policy in response.AttachedPolicies ?? [])
            {
                await client.DetachRolePolicyAsync(
                    new DetachRolePolicyRequest { RoleName = roleName, PolicyArn = policy.PolicyArn },
                    token);
            }

            marker = response.IsTruncated == true ? response.Marker : null;
        }
        while (!string.IsNullOrEmpty(marker));
    }

    private static IamRole ToRole(Role role)
        => new(
            role.RoleName ?? string.Empty,
            role.Arn ?? string.Empty,
            role.RoleId ?? string.Empty,
            role.Path ?? "/",
            ToOffset(role.CreateDate),
            string.IsNullOrEmpty(role.Description) ? null : role.Description);

    private static IamUser ToUser(User user)
        => new(
            user.UserName ?? string.Empty,
            user.Arn ?? string.Empty,
            user.UserId ?? string.Empty,
            user.Path ?? "/",
            ToOffset(user.CreateDate));

    private static IamGroup ToGroup(Group group)
        => new(
            group.GroupName ?? string.Empty,
            group.Arn ?? string.Empty,
            group.GroupId ?? string.Empty,
            group.Path ?? "/",
            ToOffset(group.CreateDate));

    private static DateTimeOffset? ToOffset(DateTime? value)
        => value is null
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));

    private static string? NormaliseLastUsed(string? value)
        => string.IsNullOrEmpty(value) || value == "N/A" ? null : value;
}
