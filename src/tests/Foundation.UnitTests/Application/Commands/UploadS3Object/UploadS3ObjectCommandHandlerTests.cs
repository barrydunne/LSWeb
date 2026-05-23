using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.UploadS3Object;
using Foundation.Application.S3;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UploadS3Object;

public class UploadS3ObjectCommandHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private UploadS3ObjectCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<UploadS3ObjectCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUploadSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        using var content = new MemoryStream([1, 2, 3]);
        _client
            .UploadObjectAsync("data", "orders/readme.txt", content, "text/plain", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UploadS3ObjectCommand("data", "orders/readme.txt", content, "text/plain"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).UploadObjectAsync(
            "data", "orders/readme.txt", content, "text/plain", Arg.Any<CancellationToken>());
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
    public async Task Handle_WhenUploadFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        using var content = new MemoryStream([1, 2, 3]);
        _client
            .UploadObjectAsync("data", "orders/readme.txt", content, "text/plain", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("upload boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UploadS3ObjectCommand("data", "orders/readme.txt", content, "text/plain"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("upload boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
