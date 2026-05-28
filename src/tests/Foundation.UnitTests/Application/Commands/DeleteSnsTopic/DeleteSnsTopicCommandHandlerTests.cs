using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.DeleteSnsTopic;
using Foundation.Application.Search;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteSnsTopic;

public class DeleteSnsTopicCommandHandlerTests
{
    private const string TopicArn = "arn:aws:sns:eu-west-1:000000000000:orders-topic";

    private readonly ISnsClient _client = Substitute.For<ISnsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private DeleteSnsTopicCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<DeleteSnsTopicCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenDeleteSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .DeleteTopicAsync(TopicArn, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DeleteSnsTopicCommand(TopicArn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).DeleteTopicAsync(TopicArn, Arg.Any<CancellationToken>());
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
    public async Task Handle_WhenDeleteFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .DeleteTopicAsync(TopicArn, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("delete boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DeleteSnsTopicCommand(TopicArn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("delete boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
