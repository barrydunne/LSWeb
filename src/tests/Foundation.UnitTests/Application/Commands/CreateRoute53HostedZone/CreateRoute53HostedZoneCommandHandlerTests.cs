using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.CreateRoute53HostedZone;
using Foundation.Application.Route53;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Route53;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateRoute53HostedZone;

public class CreateRoute53HostedZoneCommandHandlerTests
{
    private readonly IRoute53Client _client = Substitute.For<IRoute53Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private CreateRoute53HostedZoneCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateRoute53HostedZoneCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSucceeds_PublishesSuccessRefreshesSearchAndAppendsActivity()
    {
        // Arrange
        _client
            .CreateHostedZoneAsync("example.com", "demo", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HostedZone>>(new HostedZone("/hostedzone/Z1", "example.com.", 2, false)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateRoute53HostedZoneCommand("example.com", "demo"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).CreateHostedZoneAsync("example.com", "demo", Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _searchRefresh.Received(1).RequestRefresh();
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenFails_PublishesFailureAndDoesNotRefresh()
    {
        // Arrange
        _client
            .CreateHostedZoneAsync("example.com", null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HostedZone>>(new Error("zone boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateRoute53HostedZoneCommand("example.com", null),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("zone boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _searchRefresh.DidNotReceive().RequestRefresh();
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
