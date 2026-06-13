using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.PutEventBridgeRule;
using Foundation.Application.EventBridge;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.EventBridge;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutEventBridgeRule;

public class PutEventBridgeRuleCommandHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static PutEventBridgeRuleCommand BuildCommand()
        => new("orders-rule", "{\"source\":[\"my.app\"]}", "ENABLED", "desc", "bus-a");

    private PutEventBridgeRuleCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh,
            NullLogger<PutEventBridgeRuleCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSaveSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .PutEventPatternRuleAsync(Arg.Any<EventBridgeRulePatternSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).PutEventPatternRuleAsync(
            Arg.Is<EventBridgeRulePatternSpecification>(spec =>
                spec.Name == "orders-rule"
                && spec.EventPattern == "{\"source\":[\"my.app\"]}"
                && spec.State == "ENABLED"
                && spec.Description == "desc"
                && spec.EventBusName == "bus-a"),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenSaveFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .PutEventPatternRuleAsync(Arg.Any<EventBridgeRulePatternSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("rule boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("rule boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
