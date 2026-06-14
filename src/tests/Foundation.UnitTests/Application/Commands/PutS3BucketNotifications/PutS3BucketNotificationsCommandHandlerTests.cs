using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.PutS3BucketNotifications;
using Foundation.Application.S3;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.S3;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutS3BucketNotifications;

public class PutS3BucketNotificationsCommandHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private PutS3BucketNotificationsCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<PutS3BucketNotificationsCommandHandler>.Instance);

    private static IReadOnlyList<S3NotificationConfiguration> Rules()
        => [new("Queue", "arn:aws:sqs:eu-west-1:000000000000:orders", ["s3:ObjectCreated:*"], "in/", "")];

    [Fact]
    public async Task Handle_WhenSucceeds_PublishesSuccess()
    {
        // Arrange
        _client
            .PutBucketNotificationsAsync("docs", Arg.Any<IReadOnlyList<S3NotificationConfiguration>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PutS3BucketNotificationsCommand("docs", Rules()), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
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
            .PutBucketNotificationsAsync("docs", Arg.Any<IReadOnlyList<S3NotificationConfiguration>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("notif boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PutS3BucketNotificationsCommand("docs", Rules()), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("notif boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
