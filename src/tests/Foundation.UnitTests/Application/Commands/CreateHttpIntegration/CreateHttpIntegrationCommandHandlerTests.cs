using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Commands.CreateHttpIntegration;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGatewayV2;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateHttpIntegration;

public class CreateHttpIntegrationCommandHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static CreateHttpIntegrationCommand BuildCommand()
        => new("abc123", "HTTP_PROXY", "GET", "https://example.test", "1.0", "proxy");

    private CreateHttpIntegrationCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateHttpIntegrationCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .CreateIntegrationAsync(Arg.Any<HttpIntegrationSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("int1"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("int1");
        await _client.Received(1).CreateIntegrationAsync(
            Arg.Is<HttpIntegrationSpecification>(specification =>
                specification.ApiId == "abc123"
                && specification.IntegrationType == "HTTP_PROXY"),
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
            .CreateIntegrationAsync(Arg.Any<HttpIntegrationSpecification>(), Arg.Any<CancellationToken>())
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
            .CreateIntegrationAsync(Arg.Any<HttpIntegrationSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("int1"));
        var command = new CreateHttpIntegrationCommand(
            "api9", "AWS_PROXY", "POST", "arn:lambda", "2.0", "lambda proxy");
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateIntegrationAsync(
            Arg.Is<HttpIntegrationSpecification>(specification =>
                specification.ApiId == "api9"
                && specification.IntegrationType == "AWS_PROXY"
                && specification.IntegrationMethod == "POST"
                && specification.IntegrationUri == "arn:lambda"
                && specification.PayloadFormatVersion == "2.0"
                && specification.Description == "lambda proxy"),
            Arg.Any<CancellationToken>());
    }
}
