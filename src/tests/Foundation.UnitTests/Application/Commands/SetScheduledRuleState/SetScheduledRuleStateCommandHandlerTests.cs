using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.SetScheduledRuleState;
using Foundation.Application.EventBridge;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetScheduledRuleState;

public class SetScheduledRuleStateCommandHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private SetScheduledRuleStateCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<SetScheduledRuleStateCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenEnabling_CallsEnableRuleAndRefreshesSearch()
    {
        _client
            .EnableRuleAsync("daily-rule", "custom-bus", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        var result = await sut.Handle(
            new SetScheduledRuleStateCommand("daily-rule", "ENABLED", "custom-bus"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).EnableRuleAsync("daily-rule", "custom-bus", Arg.Any<CancellationToken>());
        await _client.DidNotReceive().DisableRuleAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenDisabling_CallsDisableRuleAndRefreshesSearch()
    {
        _client
            .DisableRuleAsync("daily-rule", null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        var result = await sut.Handle(
            new SetScheduledRuleStateCommand("daily-rule", "DISABLED", null),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).DisableRuleAsync("daily-rule", null, Arg.Any<CancellationToken>());
        await _client.DidNotReceive().EnableRuleAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenStateChangeFails_PublishesFailureAndReturnsError()
    {
        _client
            .EnableRuleAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("state boom")));
        var sut = CreateSut();

        var result = await sut.Handle(
            new SetScheduledRuleStateCommand("daily-rule", "ENABLED", null),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("state boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
