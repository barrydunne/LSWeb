using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.UpsertRoute53Record;
using Foundation.Application.Route53;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Route53;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpsertRoute53Record;

public class UpsertRoute53RecordCommandHandlerTests
{
    private readonly IRoute53Client _client = Substitute.For<IRoute53Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private UpsertRoute53RecordCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<UpsertRoute53RecordCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSucceeds_PublishesSuccessWithRecord()
    {
        // Arrange
        _client
            .UpsertRecordAsync(
                "/hostedzone/Z1",
                Arg.Is<Route53Record>(record => record.Name == "www.example.com." && record.Type == "A"),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UpsertRoute53RecordCommand("/hostedzone/Z1", "www.example.com.", "A", 300, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .UpsertRecordAsync("/hostedzone/Z1", Arg.Any<Route53Record>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("record boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UpsertRoute53RecordCommand("/hostedzone/Z1", "www.example.com.", "A", 300, ["1.2.3.4"]),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("record boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
