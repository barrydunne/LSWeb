using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.DeleteS3BucketPolicy;
using Foundation.Application.S3;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteS3BucketPolicy;

public class DeleteS3BucketPolicyCommandHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private DeleteS3BucketPolicyCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<DeleteS3BucketPolicyCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSucceeds_PublishesSuccess()
    {
        // Arrange
        _client
            .DeleteBucketPolicyAsync("docs", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DeleteS3BucketPolicyCommand("docs"), TestContext.Current.CancellationToken);

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
            .DeleteBucketPolicyAsync("docs", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("policy boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DeleteS3BucketPolicyCommand("docs"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("policy boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
