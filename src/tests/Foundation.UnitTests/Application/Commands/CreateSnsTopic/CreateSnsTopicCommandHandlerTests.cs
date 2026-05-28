using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.CreateSnsTopic;
using Foundation.Application.Search;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Sns;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateSnsTopic;

public class CreateSnsTopicCommandHandlerTests
{
    private readonly ISnsClient _client = Substitute.For<ISnsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static CreateSnsTopicCommand BuildCommand()
        => new("orders-topic");

    private CreateSnsTopicCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateSnsTopicCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .CreateTopicAsync(Arg.Any<SnsTopicSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).CreateTopicAsync(
            Arg.Is<SnsTopicSpecification>(spec => spec.Name == "orders-topic"),
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
            .CreateTopicAsync(Arg.Any<SnsTopicSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("create boom")));
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
    public async Task Handle_MapsCommandNameOntoSpecification()
    {
        // Arrange
        _client
            .CreateTopicAsync(Arg.Any<SnsTopicSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var command = new CreateSnsTopicCommand("alerts-topic");
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateTopicAsync(
            Arg.Is<SnsTopicSpecification>(spec => spec.Name == "alerts-topic"),
            Arg.Any<CancellationToken>());
    }
}
