using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateCognitoUser;
using Foundation.Application.Commands.CreateUserPool;
using Foundation.Application.Commands.CreateUserPoolClient;
using Foundation.Application.Commands.DeleteCognitoUser;
using Foundation.Application.Commands.DeleteUserPool;
using Foundation.Application.Commands.DeleteUserPoolClient;
using Foundation.Application.Commands.RegenerateUserPoolClientSecret;
using Foundation.Application.Commands.SetCognitoUserEnabled;
using Foundation.Application.Commands.SetCognitoUserPassword;
using Foundation.Application.Commands.UpdateUserPoolClient;
using Foundation.Application.Queries.GetUser;
using Foundation.Application.Queries.GetUserPool;
using Foundation.Application.Queries.GetUserPoolClient;
using Foundation.Application.Queries.ListUserPools;
using Foundation.Application.Queries.ListUserPoolClients;
using Foundation.Application.Queries.ListUsers;
using Foundation.Application.Queries.RequestCognitoToken;
using Foundation.Domain.Cognito;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class CognitoControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<CognitoController> _logger =
        Substitute.For<ILogger<CognitoController>>();

    private CognitoController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListUserPools_WhenQuerySucceeds_ReturnsOkWithUserPools()
    {
        // Arrange
        IReadOnlyList<UserPoolSummary> userPools =
        [
            new("eu-west-1_abc123", "customers", DateTimeOffset.UnixEpoch),
        ];
        _sender
            .Send(Arg.Any<ListUserPoolsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListUserPoolsQueryResult>>(
                new ListUserPoolsQueryResult(userPools)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListUserPools(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<UserPoolListResponse>>().Subject;
        var userPool = ok.Value!.UserPools.Should().ContainSingle().Subject;
        userPool.Id.Should().Be("eu-west-1_abc123");
        userPool.Name.Should().Be("customers");
        userPool.CreationDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task ListUserPools_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListUserPoolsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListUserPoolsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListUserPools(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetUserPool_WhenQuerySucceeds_ReturnsOkWithDetailsAndForwardsId()
    {
        // Arrange
        var detail = new UserPoolDetail(
            "eu-west-1_abc123",
            "customers",
            "arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc123",
            "OFF",
            3,
            ["email"],
            ["email"],
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch,
            new PasswordPolicy(10, true, true, true, false));
        GetUserPoolQuery? captured = null;
        _sender
            .Send(Arg.Do<GetUserPoolQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetUserPoolQueryResult>>(
                new GetUserPoolQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetUserPool("eu-west-1_abc123", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<UserPoolDetailResponse>>().Subject;
        ok.Value!.Id.Should().Be("eu-west-1_abc123");
        ok.Value.Name.Should().Be("customers");
        ok.Value.Arn.Should().Be("arn:aws:cognito-idp:eu-west-1:000000000000:userpool/eu-west-1_abc123");
        ok.Value.MfaConfiguration.Should().Be("OFF");
        ok.Value.EstimatedNumberOfUsers.Should().Be(3);
        ok.Value.UsernameAttributes.Should().ContainSingle().Which.Should().Be("email");
        ok.Value.AutoVerifiedAttributes.Should().ContainSingle().Which.Should().Be("email");
        ok.Value.CreationDate.Should().Be(DateTimeOffset.UnixEpoch);
        ok.Value.LastModifiedDate.Should().Be(DateTimeOffset.UnixEpoch);
        ok.Value.PasswordPolicy.Should().NotBeNull();
        ok.Value.PasswordPolicy!.MinimumLength.Should().Be(10);
        ok.Value.PasswordPolicy.RequireUppercase.Should().BeTrue();
        ok.Value.PasswordPolicy.RequireLowercase.Should().BeTrue();
        ok.Value.PasswordPolicy.RequireNumbers.Should().BeTrue();
        ok.Value.PasswordPolicy.RequireSymbols.Should().BeFalse();
        captured.Should().NotBeNull();
        captured!.Id.Should().Be("eu-west-1_abc123");
    }

    [Fact]
    public async Task GetUserPool_WhenPasswordPolicyNull_ReturnsNullPolicy()
    {
        // Arrange
        var detail = new UserPoolDetail(
            "eu-west-1_abc123",
            "customers",
            null,
            null,
            null,
            [],
            [],
            null,
            null,
            null);
        _sender
            .Send(Arg.Any<GetUserPoolQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetUserPoolQueryResult>>(
                new GetUserPoolQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetUserPool("eu-west-1_abc123", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<UserPoolDetailResponse>>().Subject;
        ok.Value!.PasswordPolicy.Should().BeNull();
    }

    [Fact]
    public async Task GetUserPool_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetUserPoolQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetUserPoolQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetUserPool("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateUserPool_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        var request = new UserPoolCreateRequest(
            "customers", "OFF", ["email"], ["email"],
            new PasswordPolicyModel(12, true, true, true, true));
        CreateUserPoolCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateUserPoolCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("eu-west-1_abc123"));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateUserPool(request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<UserPoolCreatedResponse>>().Subject;
        created.Value!.Id.Should().Be("eu-west-1_abc123");
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("customers");
        captured.MfaConfiguration.Should().Be("OFF");
        captured.UsernameAttributes.Should().ContainSingle().Which.Should().Be("email");
        captured.AutoVerifiedAttributes.Should().ContainSingle().Which.Should().Be("email");
        captured.PasswordPolicy.Should().NotBeNull();
        captured.PasswordPolicy!.MinimumLength.Should().Be(12);
        captured.PasswordPolicy.RequireUppercase.Should().BeTrue();
        captured.PasswordPolicy.RequireLowercase.Should().BeTrue();
        captured.PasswordPolicy.RequireNumbers.Should().BeTrue();
        captured.PasswordPolicy.RequireSymbols.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserPool_WhenAttributesNull_ForwardsEmptyLists()
    {
        // Arrange
        var request = new UserPoolCreateRequest("customers", null, null, null, null);
        CreateUserPoolCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateUserPoolCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("eu-west-1_abc123"));
        var sut = CreateSut();

        // Act
        await sut.CreateUserPool(request, TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.UsernameAttributes.Should().BeEmpty();
        captured.AutoVerifiedAttributes.Should().BeEmpty();
        captured.PasswordPolicy.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserPool_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new UserPoolCreateRequest("customers", "OFF", ["email"], ["email"], null);
        _sender
            .Send(Arg.Any<CreateUserPoolCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateUserPool(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteUserPool_WhenCommandSucceeds_ReturnsNoContentAndForwardsId()
    {
        // Arrange
        DeleteUserPoolCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteUserPoolCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUserPool("eu-west-1_abc123", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.Id.Should().Be("eu-west-1_abc123");
    }

    [Fact]
    public async Task DeleteUserPool_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteUserPoolCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUserPool("eu-west-1_abc123", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    private static UserPoolClientDetail ClientDetail()
        => new(
            "client-1",
            "web",
            "eu-west-1_abc123",
            "secret",
            true,
            ["ALLOW_USER_SRP_AUTH"],
            ["code"],
            ["openid"],
            ["https://app/callback"],
            true,
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);

    [Fact]
    public async Task ListUserPoolClients_WhenQuerySucceeds_ReturnsOkWithClientsAndForwardsPoolId()
    {
        // Arrange
        IReadOnlyList<UserPoolClientSummary> clients =
        [
            new("client-1", "web", "eu-west-1_abc123"),
        ];
        ListUserPoolClientsQuery? captured = null;
        _sender
            .Send(Arg.Do<ListUserPoolClientsQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListUserPoolClientsQueryResult>>(
                new ListUserPoolClientsQueryResult(clients)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListUserPoolClients("eu-west-1_abc123", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<UserPoolClientListResponse>>().Subject;
        var client = ok.Value!.Clients.Should().ContainSingle().Subject;
        client.ClientId.Should().Be("client-1");
        client.ClientName.Should().Be("web");
        client.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.Should().NotBeNull();
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
    }

    [Fact]
    public async Task ListUserPoolClients_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListUserPoolClientsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListUserPoolClientsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListUserPoolClients("eu-west-1_abc123", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetUserPoolClient_WhenQuerySucceeds_ReturnsOkWithDetailsAndForwardsIds()
    {
        // Arrange
        GetUserPoolClientQuery? captured = null;
        _sender
            .Send(Arg.Do<GetUserPoolClientQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetUserPoolClientQueryResult>>(
                new GetUserPoolClientQueryResult(ClientDetail())));
        var sut = CreateSut();

        // Act
        var result = await sut.GetUserPoolClient("eu-west-1_abc123", "client-1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<UserPoolClientDetailResponse>>().Subject;
        ok.Value!.ClientId.Should().Be("client-1");
        ok.Value.ClientName.Should().Be("web");
        ok.Value.UserPoolId.Should().Be("eu-west-1_abc123");
        ok.Value.ClientSecret.Should().Be("secret");
        ok.Value.GenerateSecret.Should().BeTrue();
        ok.Value.ExplicitAuthFlows.Should().ContainSingle().Which.Should().Be("ALLOW_USER_SRP_AUTH");
        ok.Value.AllowedOAuthFlows.Should().ContainSingle().Which.Should().Be("code");
        ok.Value.AllowedOAuthScopes.Should().ContainSingle().Which.Should().Be("openid");
        ok.Value.CallbackURLs.Should().ContainSingle().Which.Should().Be("https://app/callback");
        ok.Value.AllowedOAuthFlowsUserPoolClient.Should().BeTrue();
        ok.Value.CreationDate.Should().Be(DateTimeOffset.UnixEpoch);
        ok.Value.LastModifiedDate.Should().Be(DateTimeOffset.UnixEpoch);
        captured.Should().NotBeNull();
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.ClientId.Should().Be("client-1");
    }

    [Fact]
    public async Task GetUserPoolClient_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetUserPoolClientQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetUserPoolClientQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetUserPoolClient("eu-west-1_abc123", "missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateUserPoolClient_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        var request = new UserPoolClientCreateRequest(
            "web", true, ["ALLOW_USER_SRP_AUTH"], ["code"], ["openid"], ["https://app/callback"], true);
        CreateUserPoolClientCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateUserPoolClientCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(ClientDetail()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateUserPoolClient("eu-west-1_abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<UserPoolClientDetailResponse>>().Subject;
        created.Value!.ClientId.Should().Be("client-1");
        captured.Should().NotBeNull();
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.ClientName.Should().Be("web");
        captured.GenerateSecret.Should().BeTrue();
        captured.ExplicitAuthFlows.Should().ContainSingle().Which.Should().Be("ALLOW_USER_SRP_AUTH");
        captured.AllowedOAuthFlows.Should().ContainSingle().Which.Should().Be("code");
        captured.AllowedOAuthScopes.Should().ContainSingle().Which.Should().Be("openid");
        captured.CallbackURLs.Should().ContainSingle().Which.Should().Be("https://app/callback");
        captured.AllowedOAuthFlowsUserPoolClient.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserPoolClient_WhenListsNull_ForwardsEmptyLists()
    {
        // Arrange
        var request = new UserPoolClientCreateRequest("web", false, null, null, null, null, false);
        CreateUserPoolClientCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateUserPoolClientCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(ClientDetail()));
        var sut = CreateSut();

        // Act
        await sut.CreateUserPoolClient("eu-west-1_abc123", request, TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.ExplicitAuthFlows.Should().BeEmpty();
        captured.AllowedOAuthFlows.Should().BeEmpty();
        captured.AllowedOAuthScopes.Should().BeEmpty();
        captured.CallbackURLs.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateUserPoolClient_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new UserPoolClientCreateRequest("web", false, [], [], [], [], false);
        _sender
            .Send(Arg.Any<CreateUserPoolClientCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateUserPoolClient("eu-west-1_abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateUserPoolClient_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        var request = new UserPoolClientUpdateRequest(
            "web", ["ALLOW_USER_SRP_AUTH"], ["code"], ["openid"], ["https://app/callback"], true);
        UpdateUserPoolClientCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateUserPoolClientCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateUserPoolClient("eu-west-1_abc123", "client-1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.ClientId.Should().Be("client-1");
        captured.ClientName.Should().Be("web");
        captured.AllowedOAuthFlowsUserPoolClient.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserPoolClient_WhenListsNull_ForwardsEmptyLists()
    {
        // Arrange
        var request = new UserPoolClientUpdateRequest("web", null, null, null, null, false);
        UpdateUserPoolClientCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateUserPoolClientCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        await sut.UpdateUserPoolClient("eu-west-1_abc123", "client-1", request, TestContext.Current.CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.ExplicitAuthFlows.Should().BeEmpty();
        captured.AllowedOAuthFlows.Should().BeEmpty();
        captured.AllowedOAuthScopes.Should().BeEmpty();
        captured.CallbackURLs.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateUserPoolClient_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new UserPoolClientUpdateRequest("web", [], [], [], [], false);
        _sender
            .Send(Arg.Any<UpdateUserPoolClientCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateUserPoolClient("eu-west-1_abc123", "client-1", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteUserPoolClient_WhenCommandSucceeds_ReturnsNoContentAndForwardsIds()
    {
        // Arrange
        DeleteUserPoolClientCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteUserPoolClientCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUserPoolClient("eu-west-1_abc123", "client-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.ClientId.Should().Be("client-1");
    }

    [Fact]
    public async Task DeleteUserPoolClient_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteUserPoolClientCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUserPoolClient("eu-west-1_abc123", "client-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task RegenerateUserPoolClientSecret_WhenCommandSucceeds_ReturnsOkWithNewClient()
    {
        // Arrange
        var detail = new UserPoolClientDetail(
            "client-2",
            "web",
            "eu-west-1_abc123",
            "new-secret",
            true,
            ["ALLOW_USER_SRP_AUTH"],
            ["code"],
            ["openid"],
            ["https://app/callback"],
            true,
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);
        RegenerateUserPoolClientSecretCommand? captured = null;
        _sender
            .Send(Arg.Do<RegenerateUserPoolClientSecretCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(detail));
        var sut = CreateSut();

        // Act
        var result = await sut.RegenerateUserPoolClientSecret(
            "eu-west-1_abc123", "client-1", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<UserPoolClientDetailResponse>>().Subject;
        ok.Value!.ClientId.Should().Be("client-2");
        ok.Value.ClientSecret.Should().Be("new-secret");
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.ClientId.Should().Be("client-1");
    }

    [Fact]
    public async Task RegenerateUserPoolClientSecret_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RegenerateUserPoolClientSecretCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.RegenerateUserPoolClientSecret(
            "eu-west-1_abc123", "client-1", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListUsers_WhenQuerySucceeds_ReturnsOkWithUsers()
    {
        // Arrange
        IReadOnlyList<CognitoUserSummary> users =
        [
            new("alice", "CONFIRMED", true, DateTimeOffset.UnixEpoch),
        ];
        ListUsersQuery? captured = null;
        _sender
            .Send(Arg.Do<ListUsersQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListUsersQueryResult>>(new ListUsersQueryResult(users)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListUsers("eu-west-1_abc123", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CognitoUserListResponse>>().Subject;
        var user = ok.Value!.Users.Should().ContainSingle().Subject;
        user.Username.Should().Be("alice");
        user.Status.Should().Be("CONFIRMED");
        user.Enabled.Should().BeTrue();
        user.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
    }

    [Fact]
    public async Task ListUsers_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListUsersQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListUsers("eu-west-1_abc123", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetUser_WhenQuerySucceeds_ReturnsOkWithDetails()
    {
        // Arrange
        var detail = new CognitoUserDetail(
            "alice",
            "CONFIRMED",
            true,
            [new CognitoUserAttributeEntry("email", "alice@example.com")],
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);
        GetUserQuery? captured = null;
        _sender
            .Send(Arg.Do<GetUserQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetUserQueryResult>>(new GetUserQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetUser("eu-west-1_abc123", "alice", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CognitoUserDetailResponse>>().Subject;
        ok.Value!.Username.Should().Be("alice");
        ok.Value.Status.Should().Be("CONFIRMED");
        ok.Value.Enabled.Should().BeTrue();
        var attribute = ok.Value.Attributes.Should().ContainSingle().Subject;
        attribute.Name.Should().Be("email");
        attribute.Value.Should().Be("alice@example.com");
        ok.Value.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
        ok.Value.LastModifiedDate.Should().Be(DateTimeOffset.UnixEpoch);
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.Username.Should().Be("alice");
    }

    [Fact]
    public async Task GetUser_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetUserQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetUserQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetUser("eu-west-1_abc123", "alice", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateUser_WhenCommandSucceeds_ReturnsCreatedAndMapsAttributes()
    {
        // Arrange
        var detail = new CognitoUserDetail(
            "alice",
            "FORCE_CHANGE_PASSWORD",
            true,
            [new CognitoUserAttributeEntry("email", "alice@example.com")],
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);
        CreateCognitoUserCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateCognitoUserCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<CognitoUserDetail>>(detail));
        var sut = CreateSut();
        var request = new CognitoUserCreateRequest(
            "alice",
            [new CognitoUserAttributeRequest("email", "alice@example.com")],
            "Temp123!");

        // Act
        var result = await sut.CreateUser("eu-west-1_abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<CognitoUserDetailResponse>>().Subject;
        created.Value!.Username.Should().Be("alice");
        created.Location.Should().Be("/api/services/cognito/user-pools/eu-west-1_abc123/users/alice");
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.Username.Should().Be("alice");
        captured.Attributes.Should().ContainSingle(_ => _.Name == "email" && _.Value == "alice@example.com");
        captured.TemporaryPassword.Should().Be("Temp123!");
    }

    [Fact]
    public async Task CreateUser_WhenAttributesNull_DefaultsToEmpty()
    {
        // Arrange
        var detail = new CognitoUserDetail("alice", "CONFIRMED", true, [], null, null);
        CreateCognitoUserCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateCognitoUserCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<CognitoUserDetail>>(detail));
        var sut = CreateSut();
        var request = new CognitoUserCreateRequest("alice", null, null);

        // Act
        var result = await sut.CreateUser("eu-west-1_abc123", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created<CognitoUserDetailResponse>>();
        captured!.Attributes.Should().BeEmpty();
        captured.TemporaryPassword.Should().BeNull();
    }

    [Fact]
    public async Task CreateUser_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateCognitoUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<CognitoUserDetail>>(new Error("boom")));
        var sut = CreateSut();
        var request = new CognitoUserCreateRequest("alice", null, null);

        // Act
        var result = await sut.CreateUser("eu-west-1_abc123", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteUser_WhenCommandSucceeds_ReturnsNoContentAndForwardsArguments()
    {
        // Arrange
        DeleteCognitoUserCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteCognitoUserCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUser("eu-west-1_abc123", "alice", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.Username.Should().Be("alice");
    }

    [Fact]
    public async Task DeleteUser_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteCognitoUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteUser("eu-west-1_abc123", "alice", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task SetUserPassword_WhenCommandSucceeds_ReturnsNoContentAndForwardsArguments()
    {
        // Arrange
        SetCognitoUserPasswordCommand? captured = null;
        _sender
            .Send(Arg.Do<SetCognitoUserPasswordCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.SetUserPassword(
            "eu-west-1_abc123", "alice", new CognitoUserPasswordRequest("NewPass1!", true),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.Username.Should().Be("alice");
        captured.Password.Should().Be("NewPass1!");
        captured.Permanent.Should().BeTrue();
    }

    [Fact]
    public async Task SetUserPassword_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetCognitoUserPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.SetUserPassword(
            "eu-west-1_abc123", "alice", new CognitoUserPasswordRequest("NewPass1!", true),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task SetUserEnabled_WhenCommandSucceeds_ReturnsNoContentAndForwardsArguments()
    {
        // Arrange
        SetCognitoUserEnabledCommand? captured = null;
        _sender
            .Send(Arg.Do<SetCognitoUserEnabledCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.SetUserEnabled(
            "eu-west-1_abc123", "alice", new CognitoUserEnabledRequest(false),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.Username.Should().Be("alice");
        captured.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task SetUserEnabled_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetCognitoUserEnabledCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.SetUserEnabled(
            "eu-west-1_abc123", "alice", new CognitoUserEnabledRequest(true),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task RequestToken_WhenQuerySucceeds_ReturnsOkWithTokensAndClaims()
    {
        // Arrange
        var tokens = new TokenResult(
            "access-token",
            "id-token",
            "refresh-token",
            "Bearer",
            3600,
            [new CognitoUserAttributeEntry("sub", "abc")]);
        RequestCognitoTokenQuery? captured = null;
        _sender
            .Send(Arg.Do<RequestCognitoTokenQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RequestCognitoTokenQueryResult>>(
                new RequestCognitoTokenQueryResult(tokens)));
        var sut = CreateSut();

        // Act
        var result = await sut.RequestToken(
            "eu-west-1_abc123", "client-1", new CognitoTokenRequest("alice", "Passw0rd!"),
            TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CognitoTokenResponse>>().Subject;
        ok.Value!.AccessToken.Should().Be("access-token");
        ok.Value.IdToken.Should().Be("id-token");
        ok.Value.RefreshToken.Should().Be("refresh-token");
        ok.Value.TokenType.Should().Be("Bearer");
        ok.Value.ExpiresIn.Should().Be(3600);
        var claim = ok.Value.Claims.Should().ContainSingle().Subject;
        claim.Name.Should().Be("sub");
        claim.Value.Should().Be("abc");
        captured!.UserPoolId.Should().Be("eu-west-1_abc123");
        captured.ClientId.Should().Be("client-1");
        captured.Username.Should().Be("alice");
        captured.Password.Should().Be("Passw0rd!");
    }

    [Fact]
    public async Task RequestToken_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RequestCognitoTokenQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RequestCognitoTokenQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.RequestToken(
            "eu-west-1_abc123", "client-1", new CognitoTokenRequest("alice", "Passw0rd!"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
