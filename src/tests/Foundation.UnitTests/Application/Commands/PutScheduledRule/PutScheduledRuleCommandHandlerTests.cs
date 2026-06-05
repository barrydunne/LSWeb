using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.PutScheduledRule;
using Foundation.Application.EventBridge;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.EventBridge;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutScheduledRule;

public class PutScheduledRuleCommandHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static PutScheduledRuleCommand BuildCommand()
        => new("daily-rule", "rate(5 minutes)", "ENABLED", "nightly", "custom-bus");

    private PutScheduledRuleCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<PutScheduledRuleCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenPutSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        _client
            .PutRuleAsync(Arg.Any<EventBridgeRuleSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
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
    public async Task Handle_WhenPutFails_PublishesFailureAndReturnsError()
    {
        _client
            .PutRuleAsync(Arg.Any<EventBridgeRuleSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("put boom")));
        var sut = CreateSut();

        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("put boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }

    [Fact]
    public async Task Handle_MapsAllCommandFieldsOntoSpecification()
    {
        _client
            .PutRuleAsync(Arg.Any<EventBridgeRuleSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        await _client.Received(1).PutRuleAsync(
            Arg.Is<EventBridgeRuleSpecification>(spec =>
                spec.Name == "daily-rule"
                && spec.ScheduleExpression == "rate(5 minutes)"
                && spec.State == "ENABLED"
                && spec.Description == "nightly"
                && spec.EventBusName == "custom-bus"),
            Arg.Any<CancellationToken>());
    }
}
