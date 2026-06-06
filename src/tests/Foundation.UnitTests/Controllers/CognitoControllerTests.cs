using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateUserPool;
using Foundation.Application.Commands.CreateUserPoolClient;
using Foundation.Application.Commands.DeleteUserPool;
using Foundation.Application.Commands.DeleteUserPoolClient;
using Foundation.Application.Commands.UpdateUserPoolClient;
using Foundation.Application.Queries.GetUserPool;
using Foundation.Application.Queries.GetUserPoolClient;
using Foundation.Application.Queries.ListUserPools;
using Foundation.Application.Queries.ListUserPoolClients;
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
            DateTimeOffset.UnixEpoch);
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
        captured.Should().NotBeNull();
        captured!.Id.Should().Be("eu-west-1_abc123");
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
        var request = new UserPoolCreateRequest("customers", "OFF", ["email"], ["email"]);
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
    }

    [Fact]
    public async Task CreateUserPool_WhenAttributesNull_ForwardsEmptyLists()
    {
        // Arrange
        var request = new UserPoolCreateRequest("customers", null, null, null);
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
    }

    [Fact]
    public async Task CreateUserPool_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new UserPoolCreateRequest("customers", "OFF", ["email"], ["email"]);
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
}
