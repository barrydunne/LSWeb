using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.RemoveScheduledRuleTargets;
using Foundation.Application.EventBridge;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RemoveScheduledRuleTargets;

public class RemoveScheduledRuleTargetsCommandHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static RemoveScheduledRuleTargetsCommand BuildCommand()
        => new("daily-rule", "custom-bus", ["t1", "t2"]);

    private RemoveScheduledRuleTargetsCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<RemoveScheduledRuleTargetsCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenRemoveSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        _client
            .RemoveTargetsAsync(
                "daily-rule",
                "custom-bus",
                Arg.Any<IReadOnlyList<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).RemoveTargetsAsync(
            "daily-rule",
            "custom-bus",
            Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 2 && ids[0] == "t1"),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenRemoveFails_PublishesFailureAndReturnsError()
    {
        _client
            .RemoveTargetsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<IReadOnlyList<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("remove boom")));
        var sut = CreateSut();

        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("remove boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
