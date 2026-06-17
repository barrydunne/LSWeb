using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.AddUserToGroup;
using Foundation.Application.Commands.AttachGroupPolicy;
using Foundation.Application.Commands.AttachRolePolicy;
using Foundation.Application.Commands.AttachUserPolicy;
using Foundation.Application.Commands.CreateAccessKey;
using Foundation.Application.Commands.CreateAccountAlias;
using Foundation.Application.Commands.CreateGroup;
using Foundation.Application.Commands.CreatePolicy;
using Foundation.Application.Commands.CreatePolicyVersion;
using Foundation.Application.Commands.CreateRole;
using Foundation.Application.Commands.CreateUser;
using Foundation.Application.Commands.DeleteAccessKey;
using Foundation.Application.Commands.DeleteAccountAlias;
using Foundation.Application.Commands.DeleteAccountPasswordPolicy;
using Foundation.Application.Commands.DeleteGroup;
using Foundation.Application.Commands.DeleteGroupInlinePolicy;
using Foundation.Application.Commands.DeletePolicy;
using Foundation.Application.Commands.DeletePolicyVersion;
using Foundation.Application.Commands.DeleteRole;
using Foundation.Application.Commands.DeleteRoleInlinePolicy;
using Foundation.Application.Commands.DeleteRolePermissionsBoundary;
using Foundation.Application.Commands.DeleteUser;
using Foundation.Application.Commands.DeleteUserInlinePolicy;
using Foundation.Application.Commands.DeleteUserPermissionsBoundary;
using Foundation.Application.Commands.DetachGroupPolicy;
using Foundation.Application.Commands.DetachRolePolicy;
using Foundation.Application.Commands.DetachUserPolicy;
using Foundation.Application.Commands.PutGroupInlinePolicy;
using Foundation.Application.Commands.PutRoleInlinePolicy;
using Foundation.Application.Commands.PutRolePermissionsBoundary;
using Foundation.Application.Commands.PutUserInlinePolicy;
using Foundation.Application.Commands.PutUserPermissionsBoundary;
using Foundation.Application.Commands.RemoveUserFromGroup;
using Foundation.Application.Commands.SetDefaultPolicyVersion;
using Foundation.Application.Commands.TagPolicy;
using Foundation.Application.Commands.TagRole;
using Foundation.Application.Commands.TagUser;
using Foundation.Application.Commands.UntagPolicy;
using Foundation.Application.Commands.UntagRole;
using Foundation.Application.Commands.UntagUser;
using Foundation.Application.Commands.UpdateAccessKeyStatus;
using Foundation.Application.Commands.UpdateAccountPasswordPolicy;
using Foundation.Application.Commands.UpdateRole;
using Foundation.Application.Commands.UpdateRoleTrustPolicy;
using Foundation.Application.Queries.GetAccountPasswordPolicy;
using Foundation.Application.Queries.GetAccountSummary;
using Foundation.Application.Queries.GetIamGroup;
using Foundation.Application.Queries.GetIamPolicy;
using Foundation.Application.Queries.GetIamRole;
using Foundation.Application.Queries.GetIamRoleConsumers;
using Foundation.Application.Queries.GetIamUser;
using Foundation.Application.Queries.ListAccountAliases;
using Foundation.Application.Queries.ListIamGroups;
using Foundation.Application.Queries.ListIamPolicies;
using Foundation.Application.Queries.ListIamRoles;
using Foundation.Application.Queries.ListIamUsers;
using Foundation.Application.Queries.ListPolicyVersions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS Identity and Access Management users, groups, roles, and managed policies:
/// listing them, reading a single principal's detail, and managing group memberships, role trust
/// policies, attached and inline policies, policy versions, and user access keys.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/iam")]
public partial class IamController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IamController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public IamController(ISender sender, ILogger<IamController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the IAM users available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the user summaries.</returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IamUserListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListUsers(CancellationToken cancellationToken)
    {
        LogHandlingListUsers();
        var result = await _sender.Send(new ListIamUsersQuery(), cancellationToken);
        LogListUsersHandled(result.IsSuccess);
        return result.Match(
            users => Results.Ok(new IamUserListResponse(
                users.Users
                    .Select(user => new IamUserSummaryResponse(
                        user.UserName,
                        user.Arn,
                        user.UserId,
                        user.Path,
                        user.CreateDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the full detail of a single IAM user, including group memberships, policies, and access
    /// keys. The secret value of an access key is never returned.
    /// </summary>
    /// <param name="userName">The name of the user to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the user detail.</returns>
    [HttpGet("users/{userName}")]
    [ProducesResponseType(typeof(IamUserDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetUser(string userName, CancellationToken cancellationToken)
    {
        LogHandlingGetUser(userName);
        var result = await _sender.Send(new GetIamUserQuery(userName), cancellationToken);
        LogGetUserHandled(result.IsSuccess);
        return result.Match(
            detail => Results.Ok(new IamUserDetailResponse(
                detail.User.UserName,
                detail.User.Arn,
                detail.User.UserId,
                detail.User.Path,
                detail.User.CreateDate,
                detail.User.Groups,
                detail.User.AttachedPolicies
                    .Select(policy => new IamAttachedPolicyResponse(policy.PolicyName, policy.PolicyArn))
                    .ToList(),
                detail.User.InlinePolicyNames,
                detail.User.AccessKeys
                    .Select(key => new IamAccessKeyResponse(
                        key.AccessKeyId,
                        key.Status,
                        key.CreateDate,
                        key.LastUsedDate,
                        key.LastUsedService,
                        key.LastUsedRegion))
                    .ToList(),
                detail.User.Tags
                    .Select(tag => new IamTagResponse(tag.Key, tag.Value))
                    .ToList(),
                detail.User.PermissionsBoundaryArn)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new IAM user with the supplied name and optional path.
    /// </summary>
    /// <param name="request">The user to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created user.</returns>
    [HttpPost("users")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateUser(
        [FromBody] IamUserCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateUser(request.UserName);
        var result = await _sender.Send(
            new CreateUserCommand(request.UserName, request.Path), cancellationToken);
        LogCreateUserHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/iam/users/{Uri.EscapeDataString(request.UserName)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an IAM user, first removing the access keys, inline policies, group memberships, and
    /// attached managed policies that would otherwise block the deletion.
    /// </summary>
    /// <param name="userName">The name of the user to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("users/{userName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteUser(string userName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteUser(userName);
        var result = await _sender.Send(new DeleteUserCommand(userName), cancellationToken);
        LogDeleteUserHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Adds an IAM user to a group.
    /// </summary>
    /// <param name="userName">The name of the user to add.</param>
    /// <param name="request">The group to add the user to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("users/{userName}/groups")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> AddUserToGroup(
        string userName, [FromBody] IamGroupMembershipRequest request, CancellationToken cancellationToken)
    {
        LogHandlingAddUserToGroup(userName, request.GroupName);
        var result = await _sender.Send(
            new AddUserToGroupCommand(userName, request.GroupName), cancellationToken);
        LogAddUserToGroupHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Removes an IAM user from a group.
    /// </summary>
    /// <param name="userName">The name of the user to remove.</param>
    /// <param name="groupName">The name of the group to remove the user from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("users/{userName}/groups/{groupName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> RemoveUserFromGroup(
        string userName, string groupName, CancellationToken cancellationToken)
    {
        LogHandlingRemoveUserFromGroup(userName, groupName);
        var result = await _sender.Send(
            new RemoveUserFromGroupCommand(userName, groupName), cancellationToken);
        LogRemoveUserFromGroupHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Attaches a managed policy to an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user to attach the policy to.</param>
    /// <param name="request">The managed policy to attach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("users/{userName}/attached-policies")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> AttachUserPolicy(
        string userName, [FromBody] IamAttachPolicyRequest request, CancellationToken cancellationToken)
    {
        LogHandlingAttachUserPolicy(userName, request.PolicyArn);
        var result = await _sender.Send(
            new AttachUserPolicyCommand(userName, request.PolicyArn), cancellationToken);
        LogAttachUserPolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Detaches a managed policy from an IAM user. The policy ARN is supplied as a query parameter
    /// because it contains characters that are not suited to a path segment.
    /// </summary>
    /// <param name="userName">The name of the user to detach the policy from.</param>
    /// <param name="policyArn">The Amazon Resource Name of the managed policy to detach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("users/{userName}/attached-policies")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DetachUserPolicy(
        string userName, [FromQuery] string policyArn, CancellationToken cancellationToken)
    {
        LogHandlingDetachUserPolicy(userName, policyArn);
        var result = await _sender.Send(
            new DetachUserPolicyCommand(userName, policyArn), cancellationToken);
        LogDetachUserPolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Adds or updates key/value tags on an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user to tag.</param>
    /// <param name="request">The tags to add or update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("users/{userName}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> TagUser(
        string userName, [FromBody] IamTagsRequest request, CancellationToken cancellationToken)
    {
        LogHandlingTagUser(userName);
        var result = await _sender.Send(
            new TagUserCommand(
                userName,
                request.Tags.Select(tag => new Domain.Iam.IamTag(tag.Key, tag.Value)).ToList()),
            cancellationToken);
        LogTagUserHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Removes tags from an IAM user by key. The tag keys are supplied as repeated query parameters.
    /// </summary>
    /// <param name="userName">The name of the user to untag.</param>
    /// <param name="key">The keys of the tags to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("users/{userName}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UntagUser(
        string userName, [FromQuery] string[] key, CancellationToken cancellationToken)
    {
        LogHandlingUntagUser(userName);
        var result = await _sender.Send(
            new UntagUserCommand(userName, key), cancellationToken);
        LogUntagUserHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Sets the permissions boundary of an IAM user to a managed policy.
    /// </summary>
    /// <param name="userName">The name of the user to set the boundary on.</param>
    /// <param name="request">The managed policy to use as the boundary.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("users/{userName}/permissions-boundary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> PutUserPermissionsBoundary(
        string userName, [FromBody] IamPermissionsBoundaryRequest request, CancellationToken cancellationToken)
    {
        LogHandlingPutUserPermissionsBoundary(userName, request.PermissionsBoundaryArn);
        var result = await _sender.Send(
            new PutUserPermissionsBoundaryCommand(userName, request.PermissionsBoundaryArn), cancellationToken);
        LogPutUserPermissionsBoundaryHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Removes the permissions boundary from an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user to remove the boundary from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("users/{userName}/permissions-boundary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteUserPermissionsBoundary(
        string userName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteUserPermissionsBoundary(userName);
        var result = await _sender.Send(
            new DeleteUserPermissionsBoundaryCommand(userName), cancellationToken);
        LogDeleteUserPermissionsBoundaryHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Stores an inline policy document against an IAM user, creating or replacing the named policy.
    /// </summary>
    /// <param name="userName">The name of the user to store the policy against.</param>
    /// <param name="policyName">The name of the inline policy.</param>
    /// <param name="request">The policy document to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("users/{userName}/inline-policies/{policyName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> PutUserInlinePolicy(
        string userName,
        string policyName,
        [FromBody] IamInlinePolicyRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingPutUserInlinePolicy(userName, policyName);
        var result = await _sender.Send(
            new PutUserInlinePolicyCommand(userName, policyName, request.PolicyDocument),
            cancellationToken);
        LogPutUserInlinePolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a named inline policy from an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user to delete the policy from.</param>
    /// <param name="policyName">The name of the inline policy to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("users/{userName}/inline-policies/{policyName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteUserInlinePolicy(
        string userName, string policyName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteUserInlinePolicy(userName, policyName);
        var result = await _sender.Send(
            new DeleteUserInlinePolicyCommand(userName, policyName), cancellationToken);
        LogDeleteUserInlinePolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new access key for an IAM user. The secret value of the key is returned only in this
    /// response and cannot be retrieved again, so the caller must surface a copy-once warning.
    /// </summary>
    /// <param name="userName">The name of the user to create the key for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new access key and its secret.</returns>
    [HttpPost("users/{userName}/access-keys")]
    [ProducesResponseType(typeof(IamAccessKeySecretResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateAccessKey(string userName, CancellationToken cancellationToken)
    {
        LogHandlingCreateAccessKey(userName);
        var result = await _sender.Send(new CreateAccessKeyCommand(userName), cancellationToken);
        LogCreateAccessKeyHandled(result.IsSuccess);
        return result.Match(
            secret => Results.Created(
                $"/api/services/iam/users/{Uri.EscapeDataString(userName)}/access-keys/{Uri.EscapeDataString(secret.AccessKeyId)}",
                new IamAccessKeySecretResponse(
                    secret.AccessKeyId,
                    secret.SecretAccessKey,
                    secret.Status,
                    secret.CreateDate)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Changes the status of an IAM access key to Active or Inactive.
    /// </summary>
    /// <param name="userName">The name of the user that owns the key.</param>
    /// <param name="accessKeyId">The identifier of the access key to update.</param>
    /// <param name="request">The new status to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("users/{userName}/access-keys/{accessKeyId}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateAccessKeyStatus(
        string userName,
        string accessKeyId,
        [FromBody] IamAccessKeyStatusRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingUpdateAccessKeyStatus(userName, accessKeyId, request.Status);
        var result = await _sender.Send(
            new UpdateAccessKeyStatusCommand(userName, accessKeyId, request.Status), cancellationToken);
        LogUpdateAccessKeyStatusHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an access key from an IAM user.
    /// </summary>
    /// <param name="userName">The name of the user that owns the key.</param>
    /// <param name="accessKeyId">The identifier of the access key to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("users/{userName}/access-keys/{accessKeyId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteAccessKey(
        string userName, string accessKeyId, CancellationToken cancellationToken)
    {
        LogHandlingDeleteAccessKey(userName, accessKeyId);
        var result = await _sender.Send(
            new DeleteAccessKeyCommand(userName, accessKeyId), cancellationToken);
        LogDeleteAccessKeyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the IAM groups available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the group summaries.</returns>
    [HttpGet("groups")]
    [ProducesResponseType(typeof(IamGroupListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListGroups(CancellationToken cancellationToken)
    {
        LogHandlingListGroups();
        var result = await _sender.Send(new ListIamGroupsQuery(), cancellationToken);
        LogListGroupsHandled(result.IsSuccess);
        return result.Match(
            groups => Results.Ok(new IamGroupListResponse(
                groups.Groups
                    .Select(group => new IamGroupSummaryResponse(
                        group.GroupName,
                        group.Arn,
                        group.GroupId,
                        group.Path,
                        group.CreateDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the full detail of a single IAM group, including its members and attached and inline
    /// policies. Inline policies include their JSON documents.
    /// </summary>
    /// <param name="groupName">The name of the group to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the group detail.</returns>
    [HttpGet("groups/{groupName}")]
    [ProducesResponseType(typeof(IamGroupDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetGroup(string groupName, CancellationToken cancellationToken)
    {
        LogHandlingGetGroup(groupName);
        var result = await _sender.Send(new GetIamGroupQuery(groupName), cancellationToken);
        LogGetGroupHandled(result.IsSuccess);
        return result.Match(
            detail => Results.Ok(new IamGroupDetailResponse(
                detail.Group.GroupName,
                detail.Group.Arn,
                detail.Group.GroupId,
                detail.Group.Path,
                detail.Group.CreateDate,
                detail.Group.Members,
                detail.Group.AttachedPolicies
                    .Select(policy => new IamAttachedPolicyResponse(policy.PolicyName, policy.PolicyArn))
                    .ToList(),
                detail.Group.InlinePolicies
                    .Select(policy => new IamInlinePolicyResponse(policy.PolicyName, policy.PolicyDocument))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new IAM group with the supplied name and optional path.
    /// </summary>
    /// <param name="request">The group to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created group.</returns>
    [HttpPost("groups")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateGroup(
        [FromBody] IamGroupCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateGroup(request.GroupName);
        var result = await _sender.Send(
            new CreateGroupCommand(request.GroupName, request.Path), cancellationToken);
        LogCreateGroupHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/iam/groups/{Uri.EscapeDataString(request.GroupName)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an IAM group, first removing the members, inline policies, and attached managed
    /// policies that would otherwise block the deletion.
    /// </summary>
    /// <param name="groupName">The name of the group to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("groups/{groupName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteGroup(string groupName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteGroup(groupName);
        var result = await _sender.Send(new DeleteGroupCommand(groupName), cancellationToken);
        LogDeleteGroupHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Adds an IAM user to a group, identified from the group side.
    /// </summary>
    /// <param name="groupName">The name of the group to add the user to.</param>
    /// <param name="request">The user to add to the group.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("groups/{groupName}/members")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> AddGroupMember(
        string groupName, [FromBody] IamGroupMemberRequest request, CancellationToken cancellationToken)
    {
        LogHandlingAddGroupMember(groupName, request.UserName);
        var result = await _sender.Send(
            new AddUserToGroupCommand(request.UserName, groupName), cancellationToken);
        LogAddGroupMemberHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Removes an IAM user from a group, identified from the group side.
    /// </summary>
    /// <param name="groupName">The name of the group to remove the user from.</param>
    /// <param name="userName">The name of the user to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("groups/{groupName}/members/{userName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> RemoveGroupMember(
        string groupName, string userName, CancellationToken cancellationToken)
    {
        LogHandlingRemoveGroupMember(groupName, userName);
        var result = await _sender.Send(
            new RemoveUserFromGroupCommand(userName, groupName), cancellationToken);
        LogRemoveGroupMemberHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Attaches a managed policy to an IAM group.
    /// </summary>
    /// <param name="groupName">The name of the group to attach the policy to.</param>
    /// <param name="request">The managed policy to attach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("groups/{groupName}/attached-policies")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> AttachGroupPolicy(
        string groupName, [FromBody] IamAttachPolicyRequest request, CancellationToken cancellationToken)
    {
        LogHandlingAttachGroupPolicy(groupName, request.PolicyArn);
        var result = await _sender.Send(
            new AttachGroupPolicyCommand(groupName, request.PolicyArn), cancellationToken);
        LogAttachGroupPolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Detaches a managed policy from an IAM group. The policy ARN is supplied as a query parameter
    /// because it contains characters that are not suited to a path segment.
    /// </summary>
    /// <param name="groupName">The name of the group to detach the policy from.</param>
    /// <param name="policyArn">The Amazon Resource Name of the managed policy to detach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("groups/{groupName}/attached-policies")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DetachGroupPolicy(
        string groupName, [FromQuery] string policyArn, CancellationToken cancellationToken)
    {
        LogHandlingDetachGroupPolicy(groupName, policyArn);
        var result = await _sender.Send(
            new DetachGroupPolicyCommand(groupName, policyArn), cancellationToken);
        LogDetachGroupPolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Stores an inline policy document against an IAM group, creating or replacing the named policy.
    /// </summary>
    /// <param name="groupName">The name of the group to store the policy against.</param>
    /// <param name="policyName">The name of the inline policy.</param>
    /// <param name="request">The policy document to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("groups/{groupName}/inline-policies/{policyName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> PutGroupInlinePolicy(
        string groupName,
        string policyName,
        [FromBody] IamInlinePolicyRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingPutGroupInlinePolicy(groupName, policyName);
        var result = await _sender.Send(
            new PutGroupInlinePolicyCommand(groupName, policyName, request.PolicyDocument),
            cancellationToken);
        LogPutGroupInlinePolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a named inline policy from an IAM group.
    /// </summary>
    /// <param name="groupName">The name of the group to delete the policy from.</param>
    /// <param name="policyName">The name of the inline policy to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("groups/{groupName}/inline-policies/{policyName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteGroupInlinePolicy(
        string groupName, string policyName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteGroupInlinePolicy(groupName, policyName);
        var result = await _sender.Send(
            new DeleteGroupInlinePolicyCommand(groupName, policyName), cancellationToken);
        LogDeleteGroupInlinePolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the IAM roles available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the role summaries.</returns>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(IamRoleListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListRoles(CancellationToken cancellationToken)
    {
        LogHandlingListRoles();
        var result = await _sender.Send(new ListIamRolesQuery(), cancellationToken);
        LogListRolesHandled(result.IsSuccess);
        return result.Match(
            roles => Results.Ok(new IamRoleListResponse(
                roles.Roles
                    .Select(role => new IamRoleSummaryResponse(
                        role.RoleName,
                        role.Arn,
                        role.RoleId,
                        role.Path,
                        role.CreateDate,
                        role.Description))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the full detail of a single IAM role, including its trust policy and the managed and
    /// inline policies attached to it. Inline policies include their JSON documents.
    /// </summary>
    /// <param name="roleName">The name of the role to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the role detail.</returns>
    [HttpGet("roles/{roleName}")]
    [ProducesResponseType(typeof(IamRoleDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetRole(string roleName, CancellationToken cancellationToken)
    {
        LogHandlingGetRole(roleName);
        var result = await _sender.Send(new GetIamRoleQuery(roleName), cancellationToken);
        LogGetRoleHandled(result.IsSuccess);
        return result.Match(
            detail => detail.Role is null
                ? Results.NotFound()
                : Results.Ok(new IamRoleDetailResponse(
                    detail.Role.RoleName,
                    detail.Role.Arn,
                    detail.Role.RoleId,
                    detail.Role.Path,
                    detail.Role.CreateDate,
                    detail.Role.Description,
                    detail.Role.MaxSessionDuration,
                    detail.Role.AssumeRolePolicyDocument,
                    detail.Role.AttachedPolicies
                        .Select(policy => new IamAttachedPolicyResponse(policy.PolicyName, policy.PolicyArn))
                        .ToList(),
                    detail.Role.InlinePolicies
                        .Select(policy => new IamInlinePolicyResponse(policy.PolicyName, policy.PolicyDocument))
                        .ToList(),
                    detail.Role.Tags
                        .Select(tag => new IamTagResponse(tag.Key, tag.Value))
                        .ToList(),
                    detail.Role.PermissionsBoundaryArn)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the resources that use an IAM role, such as Lambda functions whose execution role
    /// matches the role. Designed so additional consumer types can be reported in future.
    /// </summary>
    /// <param name="roleName">The name of the role whose consumers to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the consumers, which may be empty.</returns>
    [HttpGet("roles/{roleName}/used-by")]
    [ProducesResponseType(typeof(IamRoleConsumersResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetRoleUsedBy(string roleName, CancellationToken cancellationToken)
    {
        LogHandlingGetRoleUsedBy(roleName);
        var result = await _sender.Send(new GetIamRoleConsumersQuery(roleName), cancellationToken);
        LogGetRoleUsedByHandled(result.IsSuccess);
        return result.Match(
            consumers => Results.Ok(new IamRoleConsumersResponse(
                consumers.Consumers
                    .Select(consumer => new IamRoleConsumerResponse(
                        consumer.ConsumerType,
                        consumer.ResourceName,
                        consumer.ServiceKey))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Adds or updates key/value tags on an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to tag.</param>
    /// <param name="request">The tags to add or update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("roles/{roleName}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> TagRole(
        string roleName, [FromBody] IamTagsRequest request, CancellationToken cancellationToken)
    {
        LogHandlingTagRole(roleName);
        var result = await _sender.Send(
            new TagRoleCommand(
                roleName,
                request.Tags.Select(tag => new Domain.Iam.IamTag(tag.Key, tag.Value)).ToList()),
            cancellationToken);
        LogTagRoleHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Removes tags from an IAM role by key. The tag keys are supplied as repeated query parameters.
    /// </summary>
    /// <param name="roleName">The name of the role to untag.</param>
    /// <param name="key">The keys of the tags to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("roles/{roleName}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UntagRole(
        string roleName, [FromQuery] string[] key, CancellationToken cancellationToken)
    {
        LogHandlingUntagRole(roleName);
        var result = await _sender.Send(
            new UntagRoleCommand(roleName, key), cancellationToken);
        LogUntagRoleHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Sets the permissions boundary of an IAM role to a managed policy.
    /// </summary>
    /// <param name="roleName">The name of the role to set the boundary on.</param>
    /// <param name="request">The managed policy to use as the boundary.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("roles/{roleName}/permissions-boundary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> PutRolePermissionsBoundary(
        string roleName, [FromBody] IamPermissionsBoundaryRequest request, CancellationToken cancellationToken)
    {
        LogHandlingPutRolePermissionsBoundary(roleName, request.PermissionsBoundaryArn);
        var result = await _sender.Send(
            new PutRolePermissionsBoundaryCommand(roleName, request.PermissionsBoundaryArn), cancellationToken);
        LogPutRolePermissionsBoundaryHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Removes the permissions boundary from an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to remove the boundary from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("roles/{roleName}/permissions-boundary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteRolePermissionsBoundary(
        string roleName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteRolePermissionsBoundary(roleName);
        var result = await _sender.Send(
            new DeleteRolePermissionsBoundaryCommand(roleName), cancellationToken);
        LogDeleteRolePermissionsBoundaryHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }
    /// <param name="request">The role to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created role.</returns>
    [HttpPost("roles")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateRole(
        [FromBody] IamRoleCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateRole(request.RoleName);
        var result = await _sender.Send(
            new CreateRoleCommand(
                request.RoleName,
                request.Path,
                request.AssumeRolePolicyDocument,
                request.Description,
                request.MaxSessionDuration),
            cancellationToken);
        LogCreateRoleHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/iam/roles/{Uri.EscapeDataString(request.RoleName)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates an IAM role. The description and maximum session duration are always applied; the
    /// trust policy is updated only when a document is supplied.
    /// </summary>
    /// <param name="roleName">The name of the role to update.</param>
    /// <param name="request">The values to update on the role.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("roles/{roleName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateRole(
        string roleName, [FromBody] IamRoleUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateRole(roleName);
        var result = await _sender.Send(
            new UpdateRoleCommand(roleName, request.Description, request.MaxSessionDuration),
            cancellationToken);
        if (result.IsSuccess && !string.IsNullOrEmpty(request.TrustPolicyDocument))
        {
            result = await _sender.Send(
                new UpdateRoleTrustPolicyCommand(roleName, request.TrustPolicyDocument), cancellationToken);
        }

        LogUpdateRoleHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an IAM role, first removing the inline policies and attached managed policies that
    /// would otherwise block the deletion.
    /// </summary>
    /// <param name="roleName">The name of the role to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("roles/{roleName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteRole(string roleName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteRole(roleName);
        var result = await _sender.Send(new DeleteRoleCommand(roleName), cancellationToken);
        LogDeleteRoleHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Attaches a managed policy to an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to attach the policy to.</param>
    /// <param name="request">The managed policy to attach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("roles/{roleName}/attached-policies")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> AttachRolePolicy(
        string roleName, [FromBody] IamAttachPolicyRequest request, CancellationToken cancellationToken)
    {
        LogHandlingAttachRolePolicy(roleName, request.PolicyArn);
        var result = await _sender.Send(
            new AttachRolePolicyCommand(roleName, request.PolicyArn), cancellationToken);
        LogAttachRolePolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Detaches a managed policy from an IAM role. The policy ARN is supplied as a query parameter
    /// because it contains characters that are not suited to a path segment.
    /// </summary>
    /// <param name="roleName">The name of the role to detach the policy from.</param>
    /// <param name="policyArn">The Amazon Resource Name of the managed policy to detach.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("roles/{roleName}/attached-policies")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DetachRolePolicy(
        string roleName, [FromQuery] string policyArn, CancellationToken cancellationToken)
    {
        LogHandlingDetachRolePolicy(roleName, policyArn);
        var result = await _sender.Send(
            new DetachRolePolicyCommand(roleName, policyArn), cancellationToken);
        LogDetachRolePolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Stores an inline policy document against an IAM role, creating or replacing the named policy.
    /// </summary>
    /// <param name="roleName">The name of the role to store the policy against.</param>
    /// <param name="policyName">The name of the inline policy.</param>
    /// <param name="request">The policy document to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("roles/{roleName}/inline-policies/{policyName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> PutRoleInlinePolicy(
        string roleName,
        string policyName,
        [FromBody] IamInlinePolicyRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingPutRoleInlinePolicy(roleName, policyName);
        var result = await _sender.Send(
            new PutRoleInlinePolicyCommand(roleName, policyName, request.PolicyDocument),
            cancellationToken);
        LogPutRoleInlinePolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a named inline policy from an IAM role.
    /// </summary>
    /// <param name="roleName">The name of the role to delete the policy from.</param>
    /// <param name="policyName">The name of the inline policy to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("roles/{roleName}/inline-policies/{policyName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteRoleInlinePolicy(
        string roleName, string policyName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteRoleInlinePolicy(roleName, policyName);
        var result = await _sender.Send(
            new DeleteRoleInlinePolicyCommand(roleName, policyName), cancellationToken);
        LogDeleteRoleInlinePolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the IAM managed policies available on the configured backend. By default only customer
    /// (local) managed policies are returned; supplying a scope of <c>aws</c> lists the AWS managed
    /// policies for use in attach pickers.
    /// </summary>
    /// <param name="scope">The policy scope to list, either <c>local</c> (default) or <c>aws</c>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the policy summaries.</returns>
    [HttpGet("policies")]
    [ProducesResponseType(typeof(IamPolicyListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListPolicies(
        [FromQuery] string? scope, CancellationToken cancellationToken)
    {
        var awsManaged = string.Equals(scope, "aws", StringComparison.OrdinalIgnoreCase);
        LogHandlingListPolicies(awsManaged);
        var result = await _sender.Send(new ListIamPoliciesQuery(awsManaged), cancellationToken);
        LogListPoliciesHandled(result.IsSuccess);
        return result.Match(
            policies => Results.Ok(new IamPolicyListResponse(
                policies.Policies
                    .Select(policy => new IamPolicySummaryResponse(
                        policy.PolicyName,
                        policy.Arn,
                        policy.PolicyId,
                        policy.Path,
                        policy.DefaultVersionId,
                        policy.AttachmentCount,
                        policy.IsAttachable,
                        policy.Description,
                        policy.CreateDate,
                        policy.UpdateDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the full detail of a single IAM managed policy, including its default version document
    /// and the list of available versions. The policy ARN is supplied as a query parameter because
    /// it contains characters that are not suited to a path segment.
    /// </summary>
    /// <param name="policyArn">The Amazon Resource Name of the policy to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the policy detail.</returns>
    [HttpGet("policies/detail")]
    [ProducesResponseType(typeof(IamPolicyDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetPolicy(
        [FromQuery] string policyArn, CancellationToken cancellationToken)
    {
        LogHandlingGetPolicy(policyArn);
        var result = await _sender.Send(new GetIamPolicyQuery(policyArn), cancellationToken);
        LogGetPolicyHandled(result.IsSuccess);
        return result.Match(
            detail => Results.Ok(new IamPolicyDetailResponse(
                detail.Policy.PolicyName,
                detail.Policy.Arn,
                detail.Policy.PolicyId,
                detail.Policy.Path,
                detail.Policy.DefaultVersionId,
                detail.Policy.AttachmentCount,
                detail.Policy.IsAttachable,
                detail.Policy.Description,
                detail.Policy.CreateDate,
                detail.Policy.UpdateDate,
                detail.Policy.DefaultVersionDocument,
                detail.Policy.Versions
                    .Select(version => new IamPolicyVersionResponse(
                        version.VersionId,
                        version.IsDefaultVersion,
                        version.CreateDate))
                    .ToList(),
                detail.Policy.Tags
                    .Select(tag => new IamTagResponse(tag.Key, tag.Value))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Adds or updates key/value tags on an IAM managed policy. The policy ARN is supplied as a
    /// query parameter because it contains characters that are not suited to a path segment.
    /// </summary>
    /// <param name="policyArn">The Amazon Resource Name of the policy to tag.</param>
    /// <param name="request">The tags to add or update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("policies/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> TagPolicy(
        [FromQuery] string policyArn, [FromBody] IamTagsRequest request, CancellationToken cancellationToken)
    {
        LogHandlingTagPolicy(policyArn);
        var result = await _sender.Send(
            new TagPolicyCommand(
                policyArn,
                request.Tags.Select(tag => new Domain.Iam.IamTag(tag.Key, tag.Value)).ToList()),
            cancellationToken);
        LogTagPolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Removes tags from an IAM managed policy by key. The policy ARN and tag keys are supplied as
    /// query parameters because the ARN contains characters that are not suited to a path segment.
    /// </summary>
    /// <param name="policyArn">The Amazon Resource Name of the policy to untag.</param>
    /// <param name="key">The keys of the tags to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("policies/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UntagPolicy(
        [FromQuery] string policyArn, [FromQuery] string[] key, CancellationToken cancellationToken)
    {
        LogHandlingUntagPolicy(policyArn);
        var result = await _sender.Send(
            new UntagPolicyCommand(policyArn, key), cancellationToken);
        LogUntagPolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the versions of an IAM managed policy. The policy ARN is supplied as a query parameter
    /// because it contains characters that are not suited to a path segment.
    /// </summary>
    /// <param name="policyArn">The Amazon Resource Name of the policy whose versions to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the policy versions.</returns>
    [HttpGet("policies/versions")]
    [ProducesResponseType(typeof(IamPolicyVersionListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListPolicyVersions(
        [FromQuery] string policyArn, CancellationToken cancellationToken)
    {
        LogHandlingListPolicyVersions(policyArn);
        var result = await _sender.Send(new ListPolicyVersionsQuery(policyArn), cancellationToken);
        LogListPolicyVersionsHandled(result.IsSuccess);
        return result.Match(
            versions => Results.Ok(new IamPolicyVersionListResponse(
                versions.Versions
                    .Select(version => new IamPolicyVersionResponse(
                        version.VersionId,
                        version.IsDefaultVersion,
                        version.CreateDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a customer managed policy with the supplied name, document, and optional description
    /// and path.
    /// </summary>
    /// <param name="request">The policy to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created policy.</returns>
    [HttpPost("policies")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreatePolicy(
        [FromBody] IamPolicyCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreatePolicy(request.PolicyName);
        var result = await _sender.Send(
            new CreatePolicyCommand(
                request.PolicyName,
                request.PolicyDocument,
                request.Description,
                request.Path),
            cancellationToken);
        LogCreatePolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/iam/policies?policyName={Uri.EscapeDataString(request.PolicyName)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new version of an existing managed policy, optionally making it the default version.
    /// </summary>
    /// <param name="request">The version to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("policies/versions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> CreatePolicyVersion(
        [FromBody] IamPolicyVersionCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreatePolicyVersion(request.PolicyArn);
        var result = await _sender.Send(
            new CreatePolicyVersionCommand(
                request.PolicyArn,
                request.PolicyDocument,
                request.SetAsDefault),
            cancellationToken);
        LogCreatePolicyVersionHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Sets the default version of a managed policy.
    /// </summary>
    /// <param name="request">The policy and version to make the default.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("policies/default-version")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> SetDefaultPolicyVersion(
        [FromBody] IamPolicyDefaultVersionRequest request, CancellationToken cancellationToken)
    {
        LogHandlingSetDefaultPolicyVersion(request.PolicyArn, request.VersionId);
        var result = await _sender.Send(
            new SetDefaultPolicyVersionCommand(request.PolicyArn, request.VersionId), cancellationToken);
        LogSetDefaultPolicyVersionHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a non-default version of a managed policy. The policy ARN and version identifier are
    /// supplied as query parameters because the ARN contains characters that are not suited to a
    /// path segment.
    /// </summary>
    /// <param name="policyArn">The Amazon Resource Name of the policy.</param>
    /// <param name="versionId">The identifier of the version to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("policies/versions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeletePolicyVersion(
        [FromQuery] string policyArn, [FromQuery] string versionId, CancellationToken cancellationToken)
    {
        LogHandlingDeletePolicyVersion(policyArn, versionId);
        var result = await _sender.Send(
            new DeletePolicyVersionCommand(policyArn, versionId), cancellationToken);
        LogDeletePolicyVersionHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a managed policy. The policy ARN is supplied as a query parameter because it contains
    /// characters that are not suited to a path segment.
    /// </summary>
    /// <param name="policyArn">The Amazon Resource Name of the policy to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("policies")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeletePolicy(
        [FromQuery] string policyArn, CancellationToken cancellationToken)
    {
        LogHandlingDeletePolicy(policyArn);
        var result = await _sender.Send(new DeletePolicyCommand(policyArn), cancellationToken);
        LogDeletePolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the account-wide IAM entity counts and quotas reported by the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the account summary.</returns>
    [HttpGet("account/summary")]
    [ProducesResponseType(typeof(IamAccountSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetAccountSummary(CancellationToken cancellationToken)
    {
        LogHandlingGetAccountSummary();
        var result = await _sender.Send(new GetAccountSummaryQuery(), cancellationToken);
        LogGetAccountSummaryHandled(result.IsSuccess);
        return result.Match(
            summary => Results.Ok(new IamAccountSummaryResponse(summary.Summary.Entries)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the account password policy. Returns HTTP 404 when no policy is set on the account.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the policy, or HTTP 404 when no policy is set.</returns>
    [HttpGet("account/password-policy")]
    [ProducesResponseType(typeof(IamPasswordPolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetAccountPasswordPolicy(CancellationToken cancellationToken)
    {
        LogHandlingGetAccountPasswordPolicy();
        var result = await _sender.Send(new GetAccountPasswordPolicyQuery(), cancellationToken);
        LogGetAccountPasswordPolicyHandled(result.IsSuccess);
        return result.Match(
            policy => policy.Policy is null
                ? Results.NotFound()
                : Results.Ok(new IamPasswordPolicyResponse(
                    policy.Policy.MinimumPasswordLength,
                    policy.Policy.RequireSymbols,
                    policy.Policy.RequireNumbers,
                    policy.Policy.RequireUppercaseCharacters,
                    policy.Policy.RequireLowercaseCharacters,
                    policy.Policy.AllowUsersToChangePassword,
                    policy.Policy.ExpirePasswords,
                    policy.Policy.MaxPasswordAge,
                    policy.Policy.PasswordReusePrevention,
                    policy.Policy.HardExpiry)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates or replaces the account password policy.
    /// </summary>
    /// <param name="request">The password policy to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("account/password-policy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateAccountPasswordPolicy(
        [FromBody] IamPasswordPolicyRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateAccountPasswordPolicy();
        var result = await _sender.Send(
            new UpdateAccountPasswordPolicyCommand(
                request.MinimumPasswordLength,
                request.RequireSymbols,
                request.RequireNumbers,
                request.RequireUppercaseCharacters,
                request.RequireLowercaseCharacters,
                request.AllowUsersToChangePassword,
                request.MaxPasswordAge,
                request.PasswordReusePrevention,
                request.HardExpiry),
            cancellationToken);
        LogUpdateAccountPasswordPolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes the account password policy.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("account/password-policy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteAccountPasswordPolicy(CancellationToken cancellationToken)
    {
        LogHandlingDeleteAccountPasswordPolicy();
        var result = await _sender.Send(new DeleteAccountPasswordPolicyCommand(), cancellationToken);
        LogDeleteAccountPasswordPolicyHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the account aliases configured on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the account aliases.</returns>
    [HttpGet("account/aliases")]
    [ProducesResponseType(typeof(IamAccountAliasListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListAccountAliases(CancellationToken cancellationToken)
    {
        LogHandlingListAccountAliases();
        var result = await _sender.Send(new ListAccountAliasesQuery(), cancellationToken);
        LogListAccountAliasesHandled(result.IsSuccess);
        return result.Match(
            aliases => Results.Ok(new IamAccountAliasListResponse(aliases.Aliases)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates an account alias.
    /// </summary>
    /// <param name="request">The account alias to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created alias.</returns>
    [HttpPost("account/aliases")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateAccountAlias(
        [FromBody] IamAccountAliasRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateAccountAlias(request.AccountAlias);
        var result = await _sender.Send(
            new CreateAccountAliasCommand(request.AccountAlias), cancellationToken);
        LogCreateAccountAliasHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/iam/account/aliases/{Uri.EscapeDataString(request.AccountAlias)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an account alias.
    /// </summary>
    /// <param name="accountAlias">The account alias to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("account/aliases/{accountAlias}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteAccountAlias(string accountAlias, CancellationToken cancellationToken)
    {
        LogHandlingDeleteAccountAlias(accountAlias);
        var result = await _sender.Send(
            new DeleteAccountAliasCommand(accountAlias), cancellationToken);
        LogDeleteAccountAliasHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling IAM user list request.")]
    private partial void LogHandlingListUsers();

    [LoggerMessage(LogLevel.Trace, "IAM user list request handled. Success: {Success}")]
    private partial void LogListUsersHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM user detail request for {UserName}.")]
    private partial void LogHandlingGetUser(string userName);

    [LoggerMessage(LogLevel.Trace, "IAM user detail request handled. Success: {Success}")]
    private partial void LogGetUserHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM user create request for {UserName}.")]
    private partial void LogHandlingCreateUser(string userName);

    [LoggerMessage(LogLevel.Trace, "IAM user create request handled. Success: {Success}")]
    private partial void LogCreateUserHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM user delete request for {UserName}.")]
    private partial void LogHandlingDeleteUser(string userName);

    [LoggerMessage(LogLevel.Trace, "IAM user delete request handled. Success: {Success}")]
    private partial void LogDeleteUserHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM add-user-to-group request for {UserName} and {GroupName}.")]
    private partial void LogHandlingAddUserToGroup(string userName, string groupName);

    [LoggerMessage(LogLevel.Trace, "IAM add-user-to-group request handled. Success: {Success}")]
    private partial void LogAddUserToGroupHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM remove-user-from-group request for {UserName} and {GroupName}.")]
    private partial void LogHandlingRemoveUserFromGroup(string userName, string groupName);

    [LoggerMessage(LogLevel.Trace, "IAM remove-user-from-group request handled. Success: {Success}")]
    private partial void LogRemoveUserFromGroupHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM attach-user-policy request for {UserName} and {PolicyArn}.")]
    private partial void LogHandlingAttachUserPolicy(string userName, string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM attach-user-policy request handled. Success: {Success}")]
    private partial void LogAttachUserPolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM detach-user-policy request for {UserName} and {PolicyArn}.")]
    private partial void LogHandlingDetachUserPolicy(string userName, string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM detach-user-policy request handled. Success: {Success}")]
    private partial void LogDetachUserPolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM put-user-inline-policy request for {UserName} and {PolicyName}.")]
    private partial void LogHandlingPutUserInlinePolicy(string userName, string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM put-user-inline-policy request handled. Success: {Success}")]
    private partial void LogPutUserInlinePolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-user-inline-policy request for {UserName} and {PolicyName}.")]
    private partial void LogHandlingDeleteUserInlinePolicy(string userName, string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM delete-user-inline-policy request handled. Success: {Success}")]
    private partial void LogDeleteUserInlinePolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM create-access-key request for {UserName}.")]
    private partial void LogHandlingCreateAccessKey(string userName);

    [LoggerMessage(LogLevel.Trace, "IAM create-access-key request handled. Success: {Success}")]
    private partial void LogCreateAccessKeyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM update-access-key-status request for {UserName} and {AccessKeyId}. Status: {Status}")]
    private partial void LogHandlingUpdateAccessKeyStatus(string userName, string accessKeyId, string status);

    [LoggerMessage(LogLevel.Trace, "IAM update-access-key-status request handled. Success: {Success}")]
    private partial void LogUpdateAccessKeyStatusHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-access-key request for {UserName} and {AccessKeyId}.")]
    private partial void LogHandlingDeleteAccessKey(string userName, string accessKeyId);

    [LoggerMessage(LogLevel.Trace, "IAM delete-access-key request handled. Success: {Success}")]
    private partial void LogDeleteAccessKeyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM group list request.")]
    private partial void LogHandlingListGroups();

    [LoggerMessage(LogLevel.Trace, "IAM group list request handled. Success: {Success}")]
    private partial void LogListGroupsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM get-group request for {GroupName}.")]
    private partial void LogHandlingGetGroup(string groupName);

    [LoggerMessage(LogLevel.Trace, "IAM get-group request handled. Success: {Success}")]
    private partial void LogGetGroupHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM create-group request for {GroupName}.")]
    private partial void LogHandlingCreateGroup(string groupName);

    [LoggerMessage(LogLevel.Trace, "IAM create-group request handled. Success: {Success}")]
    private partial void LogCreateGroupHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-group request for {GroupName}.")]
    private partial void LogHandlingDeleteGroup(string groupName);

    [LoggerMessage(LogLevel.Trace, "IAM delete-group request handled. Success: {Success}")]
    private partial void LogDeleteGroupHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM add-group-member request for {GroupName} and {UserName}.")]
    private partial void LogHandlingAddGroupMember(string groupName, string userName);

    [LoggerMessage(LogLevel.Trace, "IAM add-group-member request handled. Success: {Success}")]
    private partial void LogAddGroupMemberHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM remove-group-member request for {GroupName} and {UserName}.")]
    private partial void LogHandlingRemoveGroupMember(string groupName, string userName);

    [LoggerMessage(LogLevel.Trace, "IAM remove-group-member request handled. Success: {Success}")]
    private partial void LogRemoveGroupMemberHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM attach-group-policy request for {GroupName} and {PolicyArn}.")]
    private partial void LogHandlingAttachGroupPolicy(string groupName, string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM attach-group-policy request handled. Success: {Success}")]
    private partial void LogAttachGroupPolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM detach-group-policy request for {GroupName} and {PolicyArn}.")]
    private partial void LogHandlingDetachGroupPolicy(string groupName, string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM detach-group-policy request handled. Success: {Success}")]
    private partial void LogDetachGroupPolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM put-group-inline-policy request for {GroupName} and {PolicyName}.")]
    private partial void LogHandlingPutGroupInlinePolicy(string groupName, string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM put-group-inline-policy request handled. Success: {Success}")]
    private partial void LogPutGroupInlinePolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-group-inline-policy request for {GroupName} and {PolicyName}.")]
    private partial void LogHandlingDeleteGroupInlinePolicy(string groupName, string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM delete-group-inline-policy request handled. Success: {Success}")]
    private partial void LogDeleteGroupInlinePolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM role list request.")]
    private partial void LogHandlingListRoles();

    [LoggerMessage(LogLevel.Trace, "IAM role list request handled. Success: {Success}")]
    private partial void LogListRolesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM get-role request for {RoleName}.")]
    private partial void LogHandlingGetRole(string roleName);

    [LoggerMessage(LogLevel.Trace, "IAM get-role request handled. Success: {Success}")]
    private partial void LogGetRoleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM role used-by request for {RoleName}.")]
    private partial void LogHandlingGetRoleUsedBy(string roleName);

    [LoggerMessage(LogLevel.Trace, "IAM role used-by request handled. Success: {Success}")]
    private partial void LogGetRoleUsedByHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM create-role request for {RoleName}.")]
    private partial void LogHandlingCreateRole(string roleName);

    [LoggerMessage(LogLevel.Trace, "IAM create-role request handled. Success: {Success}")]
    private partial void LogCreateRoleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM update-role request for {RoleName}.")]
    private partial void LogHandlingUpdateRole(string roleName);

    [LoggerMessage(LogLevel.Trace, "IAM update-role request handled. Success: {Success}")]
    private partial void LogUpdateRoleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-role request for {RoleName}.")]
    private partial void LogHandlingDeleteRole(string roleName);

    [LoggerMessage(LogLevel.Trace, "IAM delete-role request handled. Success: {Success}")]
    private partial void LogDeleteRoleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM attach-role-policy request for {RoleName} and {PolicyArn}.")]
    private partial void LogHandlingAttachRolePolicy(string roleName, string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM attach-role-policy request handled. Success: {Success}")]
    private partial void LogAttachRolePolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM detach-role-policy request for {RoleName} and {PolicyArn}.")]
    private partial void LogHandlingDetachRolePolicy(string roleName, string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM detach-role-policy request handled. Success: {Success}")]
    private partial void LogDetachRolePolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM put-role-inline-policy request for {RoleName} and {PolicyName}.")]
    private partial void LogHandlingPutRoleInlinePolicy(string roleName, string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM put-role-inline-policy request handled. Success: {Success}")]
    private partial void LogPutRoleInlinePolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-role-inline-policy request for {RoleName} and {PolicyName}.")]
    private partial void LogHandlingDeleteRoleInlinePolicy(string roleName, string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM delete-role-inline-policy request handled. Success: {Success}")]
    private partial void LogDeleteRoleInlinePolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM policy list request. AwsManaged: {AwsManaged}")]
    private partial void LogHandlingListPolicies(bool awsManaged);

    [LoggerMessage(LogLevel.Trace, "IAM policy list request handled. Success: {Success}")]
    private partial void LogListPoliciesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM get-policy request for {PolicyArn}.")]
    private partial void LogHandlingGetPolicy(string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM get-policy request handled. Success: {Success}")]
    private partial void LogGetPolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM list-policy-versions request for {PolicyArn}.")]
    private partial void LogHandlingListPolicyVersions(string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM list-policy-versions request handled. Success: {Success}")]
    private partial void LogListPolicyVersionsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM create-policy request for {PolicyName}.")]
    private partial void LogHandlingCreatePolicy(string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM create-policy request handled. Success: {Success}")]
    private partial void LogCreatePolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM create-policy-version request for {PolicyArn}.")]
    private partial void LogHandlingCreatePolicyVersion(string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM create-policy-version request handled. Success: {Success}")]
    private partial void LogCreatePolicyVersionHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM set-default-policy-version request for {PolicyArn} and {VersionId}.")]
    private partial void LogHandlingSetDefaultPolicyVersion(string policyArn, string versionId);

    [LoggerMessage(LogLevel.Trace, "IAM set-default-policy-version request handled. Success: {Success}")]
    private partial void LogSetDefaultPolicyVersionHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-policy-version request for {PolicyArn} and {VersionId}.")]
    private partial void LogHandlingDeletePolicyVersion(string policyArn, string versionId);

    [LoggerMessage(LogLevel.Trace, "IAM delete-policy-version request handled. Success: {Success}")]
    private partial void LogDeletePolicyVersionHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-policy request for {PolicyArn}.")]
    private partial void LogHandlingDeletePolicy(string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM delete-policy request handled. Success: {Success}")]
    private partial void LogDeletePolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM account-summary request.")]
    private partial void LogHandlingGetAccountSummary();

    [LoggerMessage(LogLevel.Trace, "IAM account-summary request handled. Success: {Success}")]
    private partial void LogGetAccountSummaryHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM get-account-password-policy request.")]
    private partial void LogHandlingGetAccountPasswordPolicy();

    [LoggerMessage(LogLevel.Trace, "IAM get-account-password-policy request handled. Success: {Success}")]
    private partial void LogGetAccountPasswordPolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM update-account-password-policy request.")]
    private partial void LogHandlingUpdateAccountPasswordPolicy();

    [LoggerMessage(LogLevel.Trace, "IAM update-account-password-policy request handled. Success: {Success}")]
    private partial void LogUpdateAccountPasswordPolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-account-password-policy request.")]
    private partial void LogHandlingDeleteAccountPasswordPolicy();

    [LoggerMessage(LogLevel.Trace, "IAM delete-account-password-policy request handled. Success: {Success}")]
    private partial void LogDeleteAccountPasswordPolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM account-aliases list request.")]
    private partial void LogHandlingListAccountAliases();

    [LoggerMessage(LogLevel.Trace, "IAM account-aliases list request handled. Success: {Success}")]
    private partial void LogListAccountAliasesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM create-account-alias request for {AccountAlias}.")]
    private partial void LogHandlingCreateAccountAlias(string accountAlias);

    [LoggerMessage(LogLevel.Trace, "IAM create-account-alias request handled. Success: {Success}")]
    private partial void LogCreateAccountAliasHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-account-alias request for {AccountAlias}.")]
    private partial void LogHandlingDeleteAccountAlias(string accountAlias);

    [LoggerMessage(LogLevel.Trace, "IAM delete-account-alias request handled. Success: {Success}")]
    private partial void LogDeleteAccountAliasHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM tag-user request for {UserName}.")]
    private partial void LogHandlingTagUser(string userName);

    [LoggerMessage(LogLevel.Trace, "IAM tag-user request handled. Success: {Success}")]
    private partial void LogTagUserHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM untag-user request for {UserName}.")]
    private partial void LogHandlingUntagUser(string userName);

    [LoggerMessage(LogLevel.Trace, "IAM untag-user request handled. Success: {Success}")]
    private partial void LogUntagUserHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM put-user-permissions-boundary request for {UserName} and {PermissionsBoundaryArn}.")]
    private partial void LogHandlingPutUserPermissionsBoundary(string userName, string permissionsBoundaryArn);

    [LoggerMessage(LogLevel.Trace, "IAM put-user-permissions-boundary request handled. Success: {Success}")]
    private partial void LogPutUserPermissionsBoundaryHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-user-permissions-boundary request for {UserName}.")]
    private partial void LogHandlingDeleteUserPermissionsBoundary(string userName);

    [LoggerMessage(LogLevel.Trace, "IAM delete-user-permissions-boundary request handled. Success: {Success}")]
    private partial void LogDeleteUserPermissionsBoundaryHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM tag-role request for {RoleName}.")]
    private partial void LogHandlingTagRole(string roleName);

    [LoggerMessage(LogLevel.Trace, "IAM tag-role request handled. Success: {Success}")]
    private partial void LogTagRoleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM untag-role request for {RoleName}.")]
    private partial void LogHandlingUntagRole(string roleName);

    [LoggerMessage(LogLevel.Trace, "IAM untag-role request handled. Success: {Success}")]
    private partial void LogUntagRoleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM put-role-permissions-boundary request for {RoleName} and {PermissionsBoundaryArn}.")]
    private partial void LogHandlingPutRolePermissionsBoundary(string roleName, string permissionsBoundaryArn);

    [LoggerMessage(LogLevel.Trace, "IAM put-role-permissions-boundary request handled. Success: {Success}")]
    private partial void LogPutRolePermissionsBoundaryHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM delete-role-permissions-boundary request for {RoleName}.")]
    private partial void LogHandlingDeleteRolePermissionsBoundary(string roleName);

    [LoggerMessage(LogLevel.Trace, "IAM delete-role-permissions-boundary request handled. Success: {Success}")]
    private partial void LogDeleteRolePermissionsBoundaryHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM tag-policy request for {PolicyArn}.")]
    private partial void LogHandlingTagPolicy(string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM tag-policy request handled. Success: {Success}")]
    private partial void LogTagPolicyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling IAM untag-policy request for {PolicyArn}.")]
    private partial void LogHandlingUntagPolicy(string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM untag-policy request handled. Success: {Success}")]
    private partial void LogUntagPolicyHandled(bool success);
}
