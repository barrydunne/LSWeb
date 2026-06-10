using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGateway;
using Foundation.Application.Commands.CreateRestTokenAuthorizer;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGateway;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateRestTokenAuthorizer;

public class CreateRestTokenAuthorizerCommandHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static CreateRestTokenAuthorizerCommand BuildCommand()
        => new(
            "api-1",
            "jwt-authorizer",
            "https://issuer.example.com",
            "api://default",
            "method.request.header.Authorization",
            "arn:aws:apigateway:eu-west-1:lambda:path/2015-03-31/functions/authorizer/invocations");

    private CreateRestTokenAuthorizerCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateRestTokenAuthorizerCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .CreateTokenAuthorizerAsync(Arg.Any<RestTokenAuthorizerSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("auth-7"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("auth-7");
        await _client.Received(1).CreateTokenAuthorizerAsync(
            Arg.Is<RestTokenAuthorizerSpecification>(specification =>
                specification.RestApiId == "api-1"
                && specification.Name == "jwt-authorizer"
                && specification.AuthorizerUri == "arn:aws:apigateway:eu-west-1:lambda:path/2015-03-31/functions/authorizer/invocations"
                && specification.IdentitySource == "method.request.header.Authorization"),
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
            .CreateTokenAuthorizerAsync(Arg.Any<RestTokenAuthorizerSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("token boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("token boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
