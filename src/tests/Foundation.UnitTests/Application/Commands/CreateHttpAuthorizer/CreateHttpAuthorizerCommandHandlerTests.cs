using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Commands.CreateHttpAuthorizer;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGatewayV2;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateHttpAuthorizer;

public class CreateHttpAuthorizerCommandHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static CreateHttpAuthorizerCommand BuildCommand()
        => new(
            "abc123",
            "jwt-authorizer",
            "JWT",
            ["$request.header.Authorization"],
            "https://example.com/issuer",
            ["client1"]);

    private CreateHttpAuthorizerCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateHttpAuthorizerCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .CreateAuthorizerAsync(Arg.Any<HttpAuthorizerSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("auth1"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("auth1");
        await _client.Received(1).CreateAuthorizerAsync(
            Arg.Is<HttpAuthorizerSpecification>(specification =>
                specification.ApiId == "abc123"
                && specification.AuthorizerId == null
                && specification.Name == "jwt-authorizer"),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.InProgress),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenCreateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .CreateAuthorizerAsync(Arg.Any<HttpAuthorizerSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("create boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("create boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }

    [Fact]
    public async Task Handle_MapsAllCommandFieldsOntoSpecification()
    {
        // Arrange
        _client
            .CreateAuthorizerAsync(Arg.Any<HttpAuthorizerSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("auth9"));
        var command = new CreateHttpAuthorizerCommand(
            "api9",
            "name9",
            "JWT",
            ["$request.header.Authorization"],
            "https://issuer9.example.com",
            ["client9", "client10"]);
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateAuthorizerAsync(
            Arg.Is<HttpAuthorizerSpecification>(specification =>
                specification.ApiId == "api9"
                && specification.AuthorizerId == null
                && specification.Name == "name9"
                && specification.AuthorizerType == "JWT"
                && specification.IdentitySource.Contains("$request.header.Authorization")
                && specification.JwtIssuer == "https://issuer9.example.com"
                && specification.JwtAudience.Contains("client9")
                && specification.JwtAudience.Contains("client10")),
            Arg.Any<CancellationToken>());
    }
}
