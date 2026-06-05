using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.PutScheduledRuleTargets;
using Foundation.Application.EventBridge;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.EventBridge;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutScheduledRuleTargets;

public class PutScheduledRuleTargetsCommandHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static PutScheduledRuleTargetsCommand BuildCommand()
        => new(
            "daily-rule",
            "custom-bus",
            [new EventBridgeTargetSpecification("t1", "arn:aws:sqs:us-east-1:000000000000:queue", null, null)]);

    private PutScheduledRuleTargetsCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<PutScheduledRuleTargetsCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenPutSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        _client
            .PutTargetsAsync(
                "daily-rule",
                "custom-bus",
                Arg.Any<IReadOnlyList<EventBridgeTargetSpecification>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).PutTargetsAsync(
            "daily-rule",
            "custom-bus",
            Arg.Is<IReadOnlyList<EventBridgeTargetSpecification>>(targets => targets.Count == 1 && targets[0].Id == "t1"),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenPutFails_PublishesFailureAndReturnsError()
    {
        _client
            .PutTargetsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<IReadOnlyList<EventBridgeTargetSpecification>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("targets boom")));
        var sut = CreateSut();

        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("targets boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
