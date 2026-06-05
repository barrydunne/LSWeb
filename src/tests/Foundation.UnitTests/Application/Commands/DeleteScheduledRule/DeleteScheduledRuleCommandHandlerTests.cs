using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.DeleteScheduledRule;
using Foundation.Application.EventBridge;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteScheduledRule;

public class DeleteScheduledRuleCommandHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static DeleteScheduledRuleCommand BuildCommand()
        => new("daily-rule", "custom-bus");

    private DeleteScheduledRuleCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<DeleteScheduledRuleCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenDeleteSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        _client
            .DeleteRuleAsync("daily-rule", "custom-bus", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
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
        _client
            .DeleteRuleAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("delete boom")));
        var sut = CreateSut();

        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

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
