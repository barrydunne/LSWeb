using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
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
using Foundation.Domain.Iam;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class IamControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<IamController> _logger = Substitute.For<ILogger<IamController>>();

    private IamController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListUsers_WhenQuerySucceeds_ReturnsOkWithUsers()
    {
        // Arrange
        var createDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        IReadOnlyList<IamUser> users = [new("alice", "arn:alice", "AID1", "/", createDate)];
        _sender
            .Send(Arg.Any<ListIamUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListIamUsersQueryResult>>(new ListIamUsersQueryResult(users)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListUsers(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<IamUserListResponse>>().Subject;
        var user = ok.Value!.Users.Should().ContainSingle().Subject;
        user.UserName.Should().Be("alice");
        user.Arn.Should().Be("arn:alice");
        user.UserId.Should().Be("AID1");
        user.Path.Should().Be("/");
        user.CreateDate.Should().Be(createDate);
    }

    [Fact]
    public async Task ListUsers_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListIamUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListIamUsersQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListUsers(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetUser_WhenQuerySucceeds_ReturnsOkWithDetailAndForwardsName()
    {
        // Arrange
        var createDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var lastUsed = new DateTimeOffset(2024, 5, 6, 7, 8, 9, TimeSpan.Zero);
        var detail = new IamUserDetail(
            "alice",
            "arn:alice",
            "AID1",
            "/team/",
            createDate,
            ["admins"],
            [new IamAttachedPolicy("ReadOnly", "arn:aws:iam::aws:policy/ReadOnly")],
            ["inline-1"],
            [new IamAccessKey("AKIA1", "Active", createDate, lastUsed, "s3", "us-east-1")],
            [new IamTag("env", "dev")],
            "arn:aws:iam::aws:policy/Boundary");
        GetIamUserQuery? captured = null;
        _sender
            .Send(Arg.Do<GetIamUserQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamUserQueryResult>>(new GetIamUserQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetUser("alice", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<IamUserDetailResponse>>().Subject;
        ok.Value!.UserName.Should().Be("alice");
        ok.Value.Arn.Should().Be("arn:alice");
        ok.Value.UserId.Should().Be("AID1");
        ok.Value.Path.Should().Be("/team/");
        ok.Value.CreateDate.Should().Be(createDate);
        ok.Value.Groups.Should().Equal("admins");
        var policy = ok.Value.AttachedPolicies.Should().ContainSingle().Subject;
        policy.PolicyName.Should().Be("ReadOnly");
        policy.PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnly");
        ok.Value.InlinePolicyNames.Should().Equal("inline-1");
        var key = ok.Value.AccessKeys.Should().ContainSingle().Subject;
        key.AccessKeyId.Should().Be("AKIA1");
        key.Status.Should().Be("Active");
        key.CreateDate.Should().Be(createDate);
        key.LastUsedDate.Should().Be(lastUsed);
        key.LastUsedService.Should().Be("s3");
        key.LastUsedRegion.Should().Be("us-east-1");
        var tag = ok.Value.Tags.Should().ContainSingle().Subject;
        tag.Key.Should().Be("env");
        tag.Value.Should().Be("dev");
        ok.Value.PermissionsBoundaryArn.Should().Be("arn:aws:iam::aws:policy/Boundary");
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
    }

    [Fact]
    public async Task GetUser_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetIamUserQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamUserQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetUser("alice", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateUser_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        CreateUserCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateUserCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateUser(
            new IamUserCreateRequest("alice", "/team/"), TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.Path.Should().Be("/team/");
    }

    [Fact]
    public async Task CreateUser_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateUser(
            new IamUserCreateRequest("alice", null), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteUser_WhenCommandSucceeds_ReturnsNoContentAndForwardsName()
    {
        // Arrange
        DeleteUserCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteUserCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUser("alice", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
    }

    [Fact]
    public async Task DeleteUser_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUser("alice", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task AddUserToGroup_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        AddUserToGroupCommand? captured = null;
        _sender
            .Send(Arg.Do<AddUserToGroupCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.AddUserToGroup(
            "alice", new IamGroupMembershipRequest("admins"), TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.GroupName.Should().Be("admins");
    }

    [Fact]
    public async Task AddUserToGroup_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<AddUserToGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.AddUserToGroup(
            "alice", new IamGroupMembershipRequest("admins"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task RemoveUserFromGroup_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        RemoveUserFromGroupCommand? captured = null;
        _sender
            .Send(Arg.Do<RemoveUserFromGroupCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.RemoveUserFromGroup("alice", "admins", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.GroupName.Should().Be("admins");
    }

    [Fact]
    public async Task RemoveUserFromGroup_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RemoveUserFromGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.RemoveUserFromGroup("alice", "admins", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task AttachUserPolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        AttachUserPolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<AttachUserPolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.AttachUserPolicy(
            "alice",
            new IamAttachPolicyRequest("arn:aws:iam::aws:policy/ReadOnly"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnly");
    }

    [Fact]
    public async Task AttachUserPolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<AttachUserPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.AttachUserPolicy(
            "alice",
            new IamAttachPolicyRequest("arn:aws:iam::aws:policy/ReadOnly"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DetachUserPolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        DetachUserPolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<DetachUserPolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DetachUserPolicy(
            "alice", "arn:aws:iam::aws:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnly");
    }

    [Fact]
    public async Task DetachUserPolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DetachUserPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DetachUserPolicy(
            "alice", "arn:aws:iam::aws:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PutUserInlinePolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        PutUserInlinePolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<PutUserInlinePolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PutUserInlinePolicy(
            "alice",
            "inline-1",
            new IamInlinePolicyRequest("{\"Version\":\"2012-10-17\"}"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.PolicyName.Should().Be("inline-1");
        captured.PolicyDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
    }

    [Fact]
    public async Task PutUserInlinePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutUserInlinePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PutUserInlinePolicy(
            "alice",
            "inline-1",
            new IamInlinePolicyRequest("{}"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteUserInlinePolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        DeleteUserInlinePolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteUserInlinePolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUserInlinePolicy(
            "alice", "inline-1", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.PolicyName.Should().Be("inline-1");
    }

    [Fact]
    public async Task DeleteUserInlinePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteUserInlinePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUserInlinePolicy(
            "alice", "inline-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateAccessKey_WhenCommandSucceeds_ReturnsCreatedWithSecretAndForwardsName()
    {
        // Arrange
        var createDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        CreateAccessKeyCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateAccessKeyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IamAccessKeySecret>>(
                new IamAccessKeySecret("AKIA1", "topsecret", "Active", createDate)));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAccessKey("alice", TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<IamAccessKeySecretResponse>>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.Value!.AccessKeyId.Should().Be("AKIA1");
        created.Value.SecretAccessKey.Should().Be("topsecret");
        created.Value.Status.Should().Be("Active");
        created.Value.CreateDate.Should().Be(createDate);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
    }

    [Fact]
    public async Task CreateAccessKey_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateAccessKeyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IamAccessKeySecret>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAccessKey("alice", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateAccessKeyStatus_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        UpdateAccessKeyStatusCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateAccessKeyStatusCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAccessKeyStatus(
            "alice",
            "AKIA1",
            new IamAccessKeyStatusRequest("Inactive"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.AccessKeyId.Should().Be("AKIA1");
        captured.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task UpdateAccessKeyStatus_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateAccessKeyStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAccessKeyStatus(
            "alice",
            "AKIA1",
            new IamAccessKeyStatusRequest("Inactive"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteAccessKey_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        DeleteAccessKeyCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteAccessKeyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAccessKey("alice", "AKIA1", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.AccessKeyId.Should().Be("AKIA1");
    }

    [Fact]
    public async Task DeleteAccessKey_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteAccessKeyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAccessKey("alice", "AKIA1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListGroups_WhenQuerySucceeds_ReturnsOkWithGroups()
    {
        // Arrange
        var createDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        IReadOnlyList<IamGroup> groups = [new("admins", "arn:admins", "GID1", "/", createDate)];
        _sender
            .Send(Arg.Any<ListIamGroupsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListIamGroupsQueryResult>>(new ListIamGroupsQueryResult(groups)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListGroups(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<IamGroupListResponse>>().Subject;
        var group = ok.Value!.Groups.Should().ContainSingle().Subject;
        group.GroupName.Should().Be("admins");
        group.Arn.Should().Be("arn:admins");
        group.GroupId.Should().Be("GID1");
        group.Path.Should().Be("/");
        group.CreateDate.Should().Be(createDate);
    }

    [Fact]
    public async Task ListGroups_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListIamGroupsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListIamGroupsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListGroups(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetGroup_WhenQuerySucceeds_ReturnsOkWithDetailAndForwardsName()
    {
        // Arrange
        var createDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var detail = new IamGroupDetail(
            "admins",
            "arn:admins",
            "GID1",
            "/team/",
            createDate,
            ["alice", "bob"],
            [new IamAttachedPolicy("ReadOnly", "arn:aws:iam::aws:policy/ReadOnly")],
            [new IamInlinePolicy("inline-1", "{\"Version\":\"2012-10-17\"}")]);
        GetIamGroupQuery? captured = null;
        _sender
            .Send(Arg.Do<GetIamGroupQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamGroupQueryResult>>(new GetIamGroupQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetGroup("admins", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<IamGroupDetailResponse>>().Subject;
        ok.Value!.GroupName.Should().Be("admins");
        ok.Value.Arn.Should().Be("arn:admins");
        ok.Value.GroupId.Should().Be("GID1");
        ok.Value.Path.Should().Be("/team/");
        ok.Value.CreateDate.Should().Be(createDate);
        ok.Value.Members.Should().Equal("alice", "bob");
        var policy = ok.Value.AttachedPolicies.Should().ContainSingle().Subject;
        policy.PolicyName.Should().Be("ReadOnly");
        policy.PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnly");
        var inline = ok.Value.InlinePolicies.Should().ContainSingle().Subject;
        inline.PolicyName.Should().Be("inline-1");
        inline.PolicyDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
        captured.Should().NotBeNull();
        captured!.GroupName.Should().Be("admins");
    }

    [Fact]
    public async Task GetGroup_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetIamGroupQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamGroupQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetGroup("admins", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateGroup_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        CreateGroupCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateGroupCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateGroup(
            new IamGroupCreateRequest("admins", "/team/"), TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.GroupName.Should().Be("admins");
        captured.Path.Should().Be("/team/");
    }

    [Fact]
    public async Task CreateGroup_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateGroup(
            new IamGroupCreateRequest("admins", null), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteGroup_WhenCommandSucceeds_ReturnsNoContentAndForwardsName()
    {
        // Arrange
        DeleteGroupCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteGroupCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteGroup("admins", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.GroupName.Should().Be("admins");
    }

    [Fact]
    public async Task DeleteGroup_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteGroup("admins", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task AddGroupMember_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        AddUserToGroupCommand? captured = null;
        _sender
            .Send(Arg.Do<AddUserToGroupCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.AddGroupMember(
            "admins", new IamGroupMemberRequest("alice"), TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.GroupName.Should().Be("admins");
    }

    [Fact]
    public async Task AddGroupMember_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<AddUserToGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.AddGroupMember(
            "admins", new IamGroupMemberRequest("alice"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task RemoveGroupMember_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        RemoveUserFromGroupCommand? captured = null;
        _sender
            .Send(Arg.Do<RemoveUserFromGroupCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.RemoveGroupMember("admins", "alice", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.GroupName.Should().Be("admins");
    }

    [Fact]
    public async Task RemoveGroupMember_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RemoveUserFromGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.RemoveGroupMember("admins", "alice", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task AttachGroupPolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        AttachGroupPolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<AttachGroupPolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.AttachGroupPolicy(
            "admins",
            new IamAttachPolicyRequest("arn:aws:iam::aws:policy/ReadOnly"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.GroupName.Should().Be("admins");
        captured.PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnly");
    }

    [Fact]
    public async Task AttachGroupPolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<AttachGroupPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.AttachGroupPolicy(
            "admins",
            new IamAttachPolicyRequest("arn:aws:iam::aws:policy/ReadOnly"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DetachGroupPolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        DetachGroupPolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<DetachGroupPolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DetachGroupPolicy(
            "admins", "arn:aws:iam::aws:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.GroupName.Should().Be("admins");
        captured.PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnly");
    }

    [Fact]
    public async Task DetachGroupPolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DetachGroupPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DetachGroupPolicy(
            "admins", "arn:aws:iam::aws:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PutGroupInlinePolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        PutGroupInlinePolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<PutGroupInlinePolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PutGroupInlinePolicy(
            "admins",
            "inline-1",
            new IamInlinePolicyRequest("{\"Version\":\"2012-10-17\"}"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.GroupName.Should().Be("admins");
        captured.PolicyName.Should().Be("inline-1");
        captured.PolicyDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
    }

    [Fact]
    public async Task PutGroupInlinePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutGroupInlinePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PutGroupInlinePolicy(
            "admins",
            "inline-1",
            new IamInlinePolicyRequest("{}"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteGroupInlinePolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        DeleteGroupInlinePolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteGroupInlinePolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteGroupInlinePolicy(
            "admins", "inline-1", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.GroupName.Should().Be("admins");
        captured.PolicyName.Should().Be("inline-1");
    }

    [Fact]
    public async Task DeleteGroupInlinePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteGroupInlinePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteGroupInlinePolicy(
            "admins", "inline-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListRoles_WhenQuerySucceeds_ReturnsOkWithRoles()
    {
        // Arrange
        var createDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        IReadOnlyList<IamRole> roles =
            [new("svc-role", "arn:svc", "RID1", "/service/", createDate, "A service role")];
        _sender
            .Send(Arg.Any<ListIamRolesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListIamRolesQueryResult>>(new ListIamRolesQueryResult(roles)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRoles(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<IamRoleListResponse>>().Subject;
        var role = ok.Value!.Roles.Should().ContainSingle().Subject;
        role.RoleName.Should().Be("svc-role");
        role.Arn.Should().Be("arn:svc");
        role.RoleId.Should().Be("RID1");
        role.Path.Should().Be("/service/");
        role.CreateDate.Should().Be(createDate);
        role.Description.Should().Be("A service role");
    }

    [Fact]
    public async Task ListRoles_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListIamRolesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListIamRolesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRoles(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetRole_WhenQuerySucceeds_ReturnsOkWithDetailAndForwardsName()
    {
        // Arrange
        var createDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var detail = new IamRoleDetail(
            "svc-role",
            "arn:svc",
            "RID1",
            "/service/",
            createDate,
            "A service role",
            3600,
            "{\"Version\":\"2012-10-17\"}",
            [new IamAttachedPolicy("ReadOnly", "arn:aws:iam::aws:policy/ReadOnly")],
            [new IamInlinePolicy("inline-1", "{\"Statement\":[]}")],
            [new IamTag("team", "platform")],
            "arn:aws:iam::aws:policy/Boundary");
        GetIamRoleQuery? captured = null;
        _sender
            .Send(Arg.Do<GetIamRoleQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamRoleQueryResult>>(new GetIamRoleQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRole("svc-role", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<IamRoleDetailResponse>>().Subject;
        ok.Value!.RoleName.Should().Be("svc-role");
        ok.Value.Arn.Should().Be("arn:svc");
        ok.Value.RoleId.Should().Be("RID1");
        ok.Value.Path.Should().Be("/service/");
        ok.Value.CreateDate.Should().Be(createDate);
        ok.Value.Description.Should().Be("A service role");
        ok.Value.MaxSessionDuration.Should().Be(3600);
        ok.Value.AssumeRolePolicyDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
        var policy = ok.Value.AttachedPolicies.Should().ContainSingle().Subject;
        policy.PolicyName.Should().Be("ReadOnly");
        policy.PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnly");
        var inline = ok.Value.InlinePolicies.Should().ContainSingle().Subject;
        inline.PolicyName.Should().Be("inline-1");
        inline.PolicyDocument.Should().Be("{\"Statement\":[]}");
        var roleTag = ok.Value.Tags.Should().ContainSingle().Subject;
        roleTag.Key.Should().Be("team");
        roleTag.Value.Should().Be("platform");
        ok.Value.PermissionsBoundaryArn.Should().Be("arn:aws:iam::aws:policy/Boundary");
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
    }

    [Fact]
    public async Task GetRole_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetIamRoleQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamRoleQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRole("svc-role", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetRole_WhenRoleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetIamRoleQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamRoleQueryResult>>(new GetIamRoleQueryResult(null)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRole("missing-role", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetRoleUsedBy_WhenQuerySucceeds_ReturnsOkWithConsumersAndForwardsName()
    {
        // Arrange
        var consumers = new GetIamRoleConsumersQueryResult(
            [new IamRoleConsumer("Lambda function", "orders", "lambda")]);
        GetIamRoleConsumersQuery? captured = null;
        _sender
            .Send(Arg.Do<GetIamRoleConsumersQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamRoleConsumersQueryResult>>(consumers));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRoleUsedBy("svc-role", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<IamRoleConsumersResponse>>().Subject;
        var consumer = ok.Value!.Consumers.Should().ContainSingle().Subject;
        consumer.ConsumerType.Should().Be("Lambda function");
        consumer.ResourceName.Should().Be("orders");
        consumer.ServiceKey.Should().Be("lambda");
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
    }

    [Fact]
    public async Task GetRoleUsedBy_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetIamRoleConsumersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamRoleConsumersQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetRoleUsedBy("svc-role", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateRole_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        CreateRoleCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateRoleCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRole(
            new IamRoleCreateRequest(
                "svc-role", "{\"Version\":\"2012-10-17\"}", "/service/", "A service role", 3600),
            TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
        captured.Path.Should().Be("/service/");
        captured.AssumeRolePolicyDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
        captured.Description.Should().Be("A service role");
        captured.MaxSessionDuration.Should().Be(3600);
    }

    [Fact]
    public async Task CreateRole_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateRole(
            new IamRoleCreateRequest("svc-role", "{}", null, null, null),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateRole_WhenTrustPolicyOmitted_UpdatesRoleOnlyAndReturnsNoContent()
    {
        // Arrange
        UpdateRoleCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateRoleCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRole(
            "svc-role",
            new IamRoleUpdateRequest("Updated description", 7200, null),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
        captured.Description.Should().Be("Updated description");
        captured.MaxSessionDuration.Should().Be(7200);
        await _sender.DidNotReceive().Send(Arg.Any<UpdateRoleTrustPolicyCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRole_WhenTrustPolicySupplied_UpdatesRoleThenTrustPolicyAndReturnsNoContent()
    {
        // Arrange
        UpdateRoleTrustPolicyCommand? captured = null;
        _sender
            .Send(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        _sender
            .Send(Arg.Do<UpdateRoleTrustPolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRole(
            "svc-role",
            new IamRoleUpdateRequest("desc", 7200, "{\"Version\":\"2012-10-17\"}"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
        captured.PolicyDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
    }

    [Fact]
    public async Task UpdateRole_WhenUpdateFails_DoesNotUpdateTrustPolicyAndReturnsError()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRole(
            "svc-role",
            new IamRoleUpdateRequest("desc", 7200, "{\"Version\":\"2012-10-17\"}"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
        await _sender.DidNotReceive().Send(Arg.Any<UpdateRoleTrustPolicyCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRole_WhenTrustPolicyUpdateFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        _sender
            .Send(Arg.Any<UpdateRoleTrustPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateRole(
            "svc-role",
            new IamRoleUpdateRequest("desc", 7200, "{\"Version\":\"2012-10-17\"}"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteRole_WhenCommandSucceeds_ReturnsNoContentAndForwardsName()
    {
        // Arrange
        DeleteRoleCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteRoleCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRole("svc-role", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
    }

    [Fact]
    public async Task DeleteRole_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRole("svc-role", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task AttachRolePolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        AttachRolePolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<AttachRolePolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.AttachRolePolicy(
            "svc-role",
            new IamAttachPolicyRequest("arn:aws:iam::aws:policy/ReadOnly"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
        captured.PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnly");
    }

    [Fact]
    public async Task AttachRolePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<AttachRolePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.AttachRolePolicy(
            "svc-role",
            new IamAttachPolicyRequest("arn:aws:iam::aws:policy/ReadOnly"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DetachRolePolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        DetachRolePolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<DetachRolePolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DetachRolePolicy(
            "svc-role", "arn:aws:iam::aws:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
        captured.PolicyArn.Should().Be("arn:aws:iam::aws:policy/ReadOnly");
    }

    [Fact]
    public async Task DetachRolePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DetachRolePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DetachRolePolicy(
            "svc-role", "arn:aws:iam::aws:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PutRoleInlinePolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        PutRoleInlinePolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<PutRoleInlinePolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PutRoleInlinePolicy(
            "svc-role",
            "inline-1",
            new IamInlinePolicyRequest("{\"Statement\":[]}"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
        captured.PolicyName.Should().Be("inline-1");
        captured.PolicyDocument.Should().Be("{\"Statement\":[]}");
    }

    [Fact]
    public async Task PutRoleInlinePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutRoleInlinePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PutRoleInlinePolicy(
            "svc-role",
            "inline-1",
            new IamInlinePolicyRequest("{}"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteRoleInlinePolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        DeleteRoleInlinePolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteRoleInlinePolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRoleInlinePolicy(
            "svc-role", "inline-1", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
        captured.PolicyName.Should().Be("inline-1");
    }

    [Fact]
    public async Task DeleteRoleInlinePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRoleInlinePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRoleInlinePolicy(
            "svc-role", "inline-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListPolicies_WhenQuerySucceeds_ReturnsOkWithMappedPolicies()
    {
        // Arrange
        var createDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var updateDate = new DateTimeOffset(2024, 2, 3, 4, 5, 6, TimeSpan.Zero);
        IReadOnlyList<IamPolicy> policies =
        [
            new("ReadOnly", "arn:policy/ReadOnly", "PID1", "/", "v2", 4, true, "Read only", createDate, updateDate),
        ];
        ListIamPoliciesQuery? captured = null;
        _sender
            .Send(Arg.Do<ListIamPoliciesQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListIamPoliciesQueryResult>>(new ListIamPoliciesQueryResult(policies)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListPolicies("aws", TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.AwsManaged.Should().BeTrue();
        var ok = result.Should().BeOfType<Ok<IamPolicyListResponse>>().Subject;
        var policy = ok.Value!.Policies.Should().ContainSingle().Subject;
        policy.PolicyName.Should().Be("ReadOnly");
        policy.Arn.Should().Be("arn:policy/ReadOnly");
        policy.PolicyId.Should().Be("PID1");
        policy.Path.Should().Be("/");
        policy.DefaultVersionId.Should().Be("v2");
        policy.AttachmentCount.Should().Be(4);
        policy.IsAttachable.Should().BeTrue();
        policy.Description.Should().Be("Read only");
        policy.CreateDate.Should().Be(createDate);
        policy.UpdateDate.Should().Be(updateDate);
    }

    [Fact]
    public async Task ListPolicies_WhenScopeOmitted_ListsLocalPolicies()
    {
        // Arrange
        ListIamPoliciesQuery? captured = null;
        _sender
            .Send(Arg.Do<ListIamPoliciesQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListIamPoliciesQueryResult>>(
                new ListIamPoliciesQueryResult([])));
        var sut = CreateSut();

        // Act
        var result = await sut.ListPolicies(null, TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.AwsManaged.Should().BeFalse();
        result.Should().BeOfType<Ok<IamPolicyListResponse>>();
    }

    [Fact]
    public async Task ListPolicies_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListIamPoliciesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListIamPoliciesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListPolicies("aws", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetPolicy_WhenQuerySucceeds_ReturnsOkWithMappedDetail()
    {
        // Arrange
        var createDate = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var updateDate = new DateTimeOffset(2024, 2, 3, 4, 5, 6, TimeSpan.Zero);
        var versionDate = new DateTimeOffset(2024, 3, 4, 5, 6, 7, TimeSpan.Zero);
        IReadOnlyList<IamPolicyVersion> versions = [new("v2", true, versionDate)];
        var detail = new IamPolicyDetail(
            "ReadOnly", "arn:policy/ReadOnly", "PID1", "/", "v2", 4, true, "Read only",
            createDate, updateDate, "{\"Version\":\"2012-10-17\"}", versions,
            [new IamTag("team", "platform")]);
        GetIamPolicyQuery? captured = null;
        _sender
            .Send(Arg.Do<GetIamPolicyQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamPolicyQueryResult>>(new GetIamPolicyQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetPolicy("arn:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.PolicyArn.Should().Be("arn:policy/ReadOnly");
        var ok = result.Should().BeOfType<Ok<IamPolicyDetailResponse>>().Subject;
        ok.Value!.PolicyName.Should().Be("ReadOnly");
        ok.Value.Arn.Should().Be("arn:policy/ReadOnly");
        ok.Value.PolicyId.Should().Be("PID1");
        ok.Value.Path.Should().Be("/");
        ok.Value.DefaultVersionId.Should().Be("v2");
        ok.Value.AttachmentCount.Should().Be(4);
        ok.Value.IsAttachable.Should().BeTrue();
        ok.Value.Description.Should().Be("Read only");
        ok.Value.CreateDate.Should().Be(createDate);
        ok.Value.UpdateDate.Should().Be(updateDate);
        ok.Value.DefaultVersionDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
        var version = ok.Value.Versions.Should().ContainSingle().Subject;
        version.VersionId.Should().Be("v2");
        version.IsDefaultVersion.Should().BeTrue();
        version.CreateDate.Should().Be(versionDate);
        var policyTag = ok.Value.Tags.Should().ContainSingle().Subject;
        policyTag.Key.Should().Be("team");
        policyTag.Value.Should().Be("platform");
    }

    [Fact]
    public async Task GetPolicy_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetIamPolicyQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetIamPolicyQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetPolicy("arn:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListPolicyVersions_WhenQuerySucceeds_ReturnsOkWithMappedVersions()
    {
        // Arrange
        var versionDate = new DateTimeOffset(2024, 3, 4, 5, 6, 7, TimeSpan.Zero);
        IReadOnlyList<IamPolicyVersion> versions =
        [
            new("v2", true, versionDate),
            new("v1", false, null),
        ];
        ListPolicyVersionsQuery? captured = null;
        _sender
            .Send(Arg.Do<ListPolicyVersionsQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListPolicyVersionsQueryResult>>(
                new ListPolicyVersionsQueryResult(versions)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListPolicyVersions(
            "arn:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.PolicyArn.Should().Be("arn:policy/ReadOnly");
        var ok = result.Should().BeOfType<Ok<IamPolicyVersionListResponse>>().Subject;
        ok.Value!.Versions.Should().HaveCount(2);
        var first = ok.Value.Versions[0];
        first.VersionId.Should().Be("v2");
        first.IsDefaultVersion.Should().BeTrue();
        first.CreateDate.Should().Be(versionDate);
        var second = ok.Value.Versions[1];
        second.VersionId.Should().Be("v1");
        second.IsDefaultVersion.Should().BeFalse();
        second.CreateDate.Should().BeNull();
    }

    [Fact]
    public async Task ListPolicyVersions_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListPolicyVersionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListPolicyVersionsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListPolicyVersions(
            "arn:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreatePolicy_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        CreatePolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<CreatePolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreatePolicy(
            new IamPolicyCreateRequest("my-policy", "{\"Version\":\"2012-10-17\"}", "A policy", "/team/"),
            TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.PolicyName.Should().Be("my-policy");
        captured.PolicyDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
        captured.Description.Should().Be("A policy");
        captured.Path.Should().Be("/team/");
    }

    [Fact]
    public async Task CreatePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreatePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreatePolicy(
            new IamPolicyCreateRequest("my-policy", "{}", null, null),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreatePolicyVersion_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        CreatePolicyVersionCommand? captured = null;
        _sender
            .Send(Arg.Do<CreatePolicyVersionCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreatePolicyVersion(
            new IamPolicyVersionCreateRequest("arn:policy/ReadOnly", "{\"Version\":\"2012-10-17\"}", true),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.PolicyArn.Should().Be("arn:policy/ReadOnly");
        captured.PolicyDocument.Should().Be("{\"Version\":\"2012-10-17\"}");
        captured.SetAsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task CreatePolicyVersion_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreatePolicyVersionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreatePolicyVersion(
            new IamPolicyVersionCreateRequest("arn:policy/ReadOnly", "{}", false),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task SetDefaultPolicyVersion_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        SetDefaultPolicyVersionCommand? captured = null;
        _sender
            .Send(Arg.Do<SetDefaultPolicyVersionCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.SetDefaultPolicyVersion(
            new IamPolicyDefaultVersionRequest("arn:policy/ReadOnly", "v2"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.PolicyArn.Should().Be("arn:policy/ReadOnly");
        captured.VersionId.Should().Be("v2");
    }

    [Fact]
    public async Task SetDefaultPolicyVersion_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetDefaultPolicyVersionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.SetDefaultPolicyVersion(
            new IamPolicyDefaultVersionRequest("arn:policy/ReadOnly", "v2"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeletePolicyVersion_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        DeletePolicyVersionCommand? captured = null;
        _sender
            .Send(Arg.Do<DeletePolicyVersionCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeletePolicyVersion(
            "arn:policy/ReadOnly", "v1", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.PolicyArn.Should().Be("arn:policy/ReadOnly");
        captured.VersionId.Should().Be("v1");
    }

    [Fact]
    public async Task DeletePolicyVersion_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeletePolicyVersionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeletePolicyVersion(
            "arn:policy/ReadOnly", "v1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeletePolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsArn()
    {
        // Arrange
        DeletePolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<DeletePolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeletePolicy(
            "arn:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.PolicyArn.Should().Be("arn:policy/ReadOnly");
    }

    [Fact]
    public async Task DeletePolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeletePolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeletePolicy(
            "arn:policy/ReadOnly", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetAccountSummary_WhenQuerySucceeds_ReturnsOkWithEntries()
    {
        // Arrange
        var summary = new IamAccountSummary(new Dictionary<string, int> { ["Users"] = 3, ["UsersQuota"] = 5000 });
        _sender
            .Send(Arg.Any<GetAccountSummaryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetAccountSummaryQueryResult>>(
                new GetAccountSummaryQueryResult(summary)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetAccountSummary(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<IamAccountSummaryResponse>>().Subject;
        ok.Value!.Entries.Should().BeEquivalentTo(new Dictionary<string, int> { ["Users"] = 3, ["UsersQuota"] = 5000 });
    }

    [Fact]
    public async Task GetAccountSummary_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetAccountSummaryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetAccountSummaryQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetAccountSummary(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetAccountPasswordPolicy_WhenPolicyExists_ReturnsOkWithPolicy()
    {
        // Arrange
        var policy = new IamPasswordPolicy(14, true, true, true, true, true, true, 90, 5, false);
        _sender
            .Send(Arg.Any<GetAccountPasswordPolicyQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetAccountPasswordPolicyQueryResult>>(
                new GetAccountPasswordPolicyQueryResult(policy)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetAccountPasswordPolicy(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<IamPasswordPolicyResponse>>().Subject;
        ok.Value!.MinimumPasswordLength.Should().Be(14);
        ok.Value.RequireSymbols.Should().BeTrue();
        ok.Value.RequireNumbers.Should().BeTrue();
        ok.Value.RequireUppercaseCharacters.Should().BeTrue();
        ok.Value.RequireLowercaseCharacters.Should().BeTrue();
        ok.Value.AllowUsersToChangePassword.Should().BeTrue();
        ok.Value.ExpirePasswords.Should().BeTrue();
        ok.Value.MaxPasswordAge.Should().Be(90);
        ok.Value.PasswordReusePrevention.Should().Be(5);
        ok.Value.HardExpiry.Should().BeFalse();
    }

    [Fact]
    public async Task GetAccountPasswordPolicy_WhenNoPolicy_ReturnsNotFound()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetAccountPasswordPolicyQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetAccountPasswordPolicyQueryResult>>(
                new GetAccountPasswordPolicyQueryResult(null)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetAccountPasswordPolicy(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetAccountPasswordPolicy_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetAccountPasswordPolicyQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetAccountPasswordPolicyQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetAccountPasswordPolicy(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateAccountPasswordPolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        UpdateAccountPasswordPolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateAccountPasswordPolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAccountPasswordPolicy(
            new IamPasswordPolicyRequest(12, true, false, true, true, false, 60, 3, true),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.MinimumPasswordLength.Should().Be(12);
        captured.RequireSymbols.Should().BeTrue();
        captured.RequireNumbers.Should().BeFalse();
        captured.RequireUppercaseCharacters.Should().BeTrue();
        captured.RequireLowercaseCharacters.Should().BeTrue();
        captured.AllowUsersToChangePassword.Should().BeFalse();
        captured.MaxPasswordAge.Should().Be(60);
        captured.PasswordReusePrevention.Should().Be(3);
        captured.HardExpiry.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAccountPasswordPolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateAccountPasswordPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAccountPasswordPolicy(
            new IamPasswordPolicyRequest(12, true, false, true, true, false, null, null, false),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteAccountPasswordPolicy_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteAccountPasswordPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAccountPasswordPolicy(TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task DeleteAccountPasswordPolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteAccountPasswordPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAccountPasswordPolicy(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListAccountAliases_WhenQuerySucceeds_ReturnsOkWithAliases()
    {
        // Arrange
        IReadOnlyList<string> aliases = ["my-account"];
        _sender
            .Send(Arg.Any<ListAccountAliasesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListAccountAliasesQueryResult>>(
                new ListAccountAliasesQueryResult(aliases)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListAccountAliases(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<IamAccountAliasListResponse>>().Subject;
        ok.Value!.Aliases.Should().Equal("my-account");
    }

    [Fact]
    public async Task ListAccountAliases_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListAccountAliasesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListAccountAliasesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListAccountAliases(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateAccountAlias_WhenCommandSucceeds_ReturnsCreatedAndForwardsAlias()
    {
        // Arrange
        CreateAccountAliasCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateAccountAliasCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAccountAlias(
            new IamAccountAliasRequest("my-account"), TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.AccountAlias.Should().Be("my-account");
    }

    [Fact]
    public async Task CreateAccountAlias_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateAccountAliasCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAccountAlias(
            new IamAccountAliasRequest("my-account"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteAccountAlias_WhenCommandSucceeds_ReturnsNoContentAndForwardsAlias()
    {
        // Arrange
        DeleteAccountAliasCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteAccountAliasCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAccountAlias("my-account", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.AccountAlias.Should().Be("my-account");
    }

    [Fact]
    public async Task DeleteAccountAlias_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteAccountAliasCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAccountAlias("my-account", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task TagUser_WhenCommandSucceeds_ReturnsNoContentAndForwardsTags()
    {
        // Arrange
        TagUserCommand? captured = null;
        _sender
            .Send(Arg.Do<TagUserCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.TagUser(
            "alice",
            new IamTagsRequest([new IamTagRequest("env", "dev")]),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        var tag = captured.Tags.Should().ContainSingle().Subject;
        tag.Key.Should().Be("env");
        tag.Value.Should().Be("dev");
    }

    [Fact]
    public async Task TagUser_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<TagUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.TagUser(
            "alice",
            new IamTagsRequest([new IamTagRequest("env", "dev")]),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UntagUser_WhenCommandSucceeds_ReturnsNoContentAndForwardsKeys()
    {
        // Arrange
        UntagUserCommand? captured = null;
        _sender
            .Send(Arg.Do<UntagUserCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UntagUser(
            "alice", ["env", "team"], TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.TagKeys.Should().Equal("env", "team");
    }

    [Fact]
    public async Task UntagUser_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UntagUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UntagUser(
            "alice", ["env"], TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PutUserPermissionsBoundary_WhenCommandSucceeds_ReturnsNoContentAndForwardsArn()
    {
        // Arrange
        PutUserPermissionsBoundaryCommand? captured = null;
        _sender
            .Send(Arg.Do<PutUserPermissionsBoundaryCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PutUserPermissionsBoundary(
            "alice",
            new IamPermissionsBoundaryRequest("arn:aws:iam::aws:policy/Boundary"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
        captured.PermissionsBoundaryArn.Should().Be("arn:aws:iam::aws:policy/Boundary");
    }

    [Fact]
    public async Task PutUserPermissionsBoundary_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutUserPermissionsBoundaryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PutUserPermissionsBoundary(
            "alice",
            new IamPermissionsBoundaryRequest("arn:aws:iam::aws:policy/Boundary"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteUserPermissionsBoundary_WhenCommandSucceeds_ReturnsNoContentAndForwardsName()
    {
        // Arrange
        DeleteUserPermissionsBoundaryCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteUserPermissionsBoundaryCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUserPermissionsBoundary(
            "alice", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserName.Should().Be("alice");
    }

    [Fact]
    public async Task DeleteUserPermissionsBoundary_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteUserPermissionsBoundaryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUserPermissionsBoundary(
            "alice", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task TagRole_WhenCommandSucceeds_ReturnsNoContentAndForwardsTags()
    {
        // Arrange
        TagRoleCommand? captured = null;
        _sender
            .Send(Arg.Do<TagRoleCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.TagRole(
            "svc-role",
            new IamTagsRequest([new IamTagRequest("team", "platform")]),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
        var tag = captured.Tags.Should().ContainSingle().Subject;
        tag.Key.Should().Be("team");
        tag.Value.Should().Be("platform");
    }

    [Fact]
    public async Task TagRole_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<TagRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.TagRole(
            "svc-role",
            new IamTagsRequest([new IamTagRequest("team", "platform")]),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UntagRole_WhenCommandSucceeds_ReturnsNoContentAndForwardsKeys()
    {
        // Arrange
        UntagRoleCommand? captured = null;
        _sender
            .Send(Arg.Do<UntagRoleCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UntagRole(
            "svc-role", ["team"], TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
        captured.TagKeys.Should().Equal("team");
    }

    [Fact]
    public async Task UntagRole_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UntagRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UntagRole(
            "svc-role", ["team"], TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PutRolePermissionsBoundary_WhenCommandSucceeds_ReturnsNoContentAndForwardsArn()
    {
        // Arrange
        PutRolePermissionsBoundaryCommand? captured = null;
        _sender
            .Send(Arg.Do<PutRolePermissionsBoundaryCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PutRolePermissionsBoundary(
            "svc-role",
            new IamPermissionsBoundaryRequest("arn:aws:iam::aws:policy/Boundary"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
        captured.PermissionsBoundaryArn.Should().Be("arn:aws:iam::aws:policy/Boundary");
    }

    [Fact]
    public async Task PutRolePermissionsBoundary_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutRolePermissionsBoundaryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PutRolePermissionsBoundary(
            "svc-role",
            new IamPermissionsBoundaryRequest("arn:aws:iam::aws:policy/Boundary"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteRolePermissionsBoundary_WhenCommandSucceeds_ReturnsNoContentAndForwardsName()
    {
        // Arrange
        DeleteRolePermissionsBoundaryCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteRolePermissionsBoundaryCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRolePermissionsBoundary(
            "svc-role", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.RoleName.Should().Be("svc-role");
    }

    [Fact]
    public async Task DeleteRolePermissionsBoundary_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteRolePermissionsBoundaryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteRolePermissionsBoundary(
            "svc-role", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task TagPolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsTags()
    {
        // Arrange
        TagPolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<TagPolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.TagPolicy(
            "arn:policy/ReadOnly",
            new IamTagsRequest([new IamTagRequest("team", "platform")]),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.PolicyArn.Should().Be("arn:policy/ReadOnly");
        var tag = captured.Tags.Should().ContainSingle().Subject;
        tag.Key.Should().Be("team");
        tag.Value.Should().Be("platform");
    }

    [Fact]
    public async Task TagPolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<TagPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.TagPolicy(
            "arn:policy/ReadOnly",
            new IamTagsRequest([new IamTagRequest("team", "platform")]),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UntagPolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsKeys()
    {
        // Arrange
        UntagPolicyCommand? captured = null;
        _sender
            .Send(Arg.Do<UntagPolicyCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UntagPolicy(
            "arn:policy/ReadOnly", ["team"], TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.PolicyArn.Should().Be("arn:policy/ReadOnly");
        captured.TagKeys.Should().Equal("team");
    }

    [Fact]
    public async Task UntagPolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UntagPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UntagPolicy(
            "arn:policy/ReadOnly", ["team"], TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
