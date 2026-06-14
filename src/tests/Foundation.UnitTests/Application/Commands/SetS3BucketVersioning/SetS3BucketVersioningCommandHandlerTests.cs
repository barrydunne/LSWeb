using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.SetS3BucketVersioning;
using Foundation.Application.S3;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetS3BucketVersioning;

public class SetS3BucketVersioningCommandHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private SetS3BucketVersioningCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<SetS3BucketVersioningCommandHandler>.Instance);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_WhenSucceeds_PublishesSuccess(bool enabled)
    {
        // Arrange
        _client
            .SetBucketVersioningAsync("docs", enabled, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new SetS3BucketVersioningCommand("docs", enabled), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_WhenFails_PublishesFailureAndReturnsError(bool enabled)
    {
        // Arrange
        _client
            .SetBucketVersioningAsync("docs", enabled, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("versioning boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new SetS3BucketVersioningCommand("docs", enabled), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("versioning boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
