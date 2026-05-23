using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.MoveS3Object;
using Foundation.Application.S3;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.MoveS3Object;

public class MoveS3ObjectCommandHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private MoveS3ObjectCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<MoveS3ObjectCommandHandler>.Instance);

    private static MoveS3ObjectCommand Command()
        => new("data", "orders/readme.txt", "archive", "orders/2026/readme.txt");

    [Fact]
    public async Task Handle_WhenCopyAndDeleteSucceed_PublishesSuccessAppendsActivityAndRefreshesSearch()
    {
        // Arrange
        _client
            .CopyObjectAsync(
                "data", "orders/readme.txt", "archive", "orders/2026/readme.txt", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        _client
            .DeleteObjectAsync("data", "orders/readme.txt", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).CopyObjectAsync(
            "data", "orders/readme.txt", "archive", "orders/2026/readme.txt", Arg.Any<CancellationToken>());
        await _client.Received(1).DeleteObjectAsync("data", "orders/readme.txt", Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenCopyFails_PublishesFailureAndReturnsErrorWithoutDeleteOrRefresh()
    {
        // Arrange
        _client
            .CopyObjectAsync(
                "data", "orders/readme.txt", "archive", "orders/2026/readme.txt", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("copy boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("copy boom");
        await _client.DidNotReceive().DeleteObjectAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenDeleteFails_PublishesFailureRefreshesSearchAndReturnsError()
    {
        // Arrange
        _client
            .CopyObjectAsync(
                "data", "orders/readme.txt", "archive", "orders/2026/readme.txt", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        _client
            .DeleteObjectAsync("data", "orders/readme.txt", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("delete boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("delete boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.Received(1).RequestRefresh();
    }
}
