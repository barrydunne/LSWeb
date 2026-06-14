using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.UpdateLambdaFunctionUrl;
using Foundation.Application.Lambda;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Lambda;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateLambdaFunctionUrl;

public class UpdateLambdaFunctionUrlCommandHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private UpdateLambdaFunctionUrlCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<UpdateLambdaFunctionUrlCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSucceeds_PublishesSuccessAndAppendsActivity()
    {
        // Arrange
        _client
            .UpdateFunctionUrlAsync("orders", "AWS_IAM", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionUrl>>(
                new LambdaFunctionUrl("https://abc.lambda-url.eu-west-1.on.aws/", "AWS_IAM", "t1", "t2")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UpdateLambdaFunctionUrlCommand("orders", "AWS_IAM"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).UpdateFunctionUrlAsync("orders", "AWS_IAM", Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .UpdateFunctionUrlAsync("orders", "AWS_IAM", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionUrl>>(new Error("url boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UpdateLambdaFunctionUrlCommand("orders", "AWS_IAM"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("url boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
