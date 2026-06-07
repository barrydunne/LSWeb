using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.ApplySeedTemplate;
using Foundation.Application.Commands.CreateSnsTopic;
using Foundation.Application.Seed;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Seed;
using Foundation.Domain.Streaming;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using INotificationPublisher = Foundation.Application.Streaming.INotificationPublisher;

namespace Foundation.UnitTests.Application.Commands.ApplySeedTemplate;

public class ApplySeedTemplateCommandHandlerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly SeedTemplateCatalogue _catalogue = new();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private ApplySeedTemplateCommandHandler CreateSut()
        => new(_sender, _catalogue, _publisher, _activityLog, NullLogger<ApplySeedTemplateCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenAllResourcesCreated_ReturnsSucceededOutcome()
    {
        // Arrange
        _sender.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var published = new List<Notification>();
        _publisher
            .PublishAsync(Arg.Do<Notification>(_ => published.Add(_)), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var appended = new List<ActivityEntry>();
        _activityLog.When(_ => _.Append(Arg.Any<ActivityEntry>())).Do(_ => appended.Add(_.Arg<ActivityEntry>()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ApplySeedTemplateCommand("messaging-starter"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TemplateId.Should().Be("messaging-starter");
        result.Value.TotalCount.Should().Be(2);
        result.Value.SucceededCount.Should().Be(2);
        result.Value.FailedCount.Should().Be(0);
        result.Value.OverallState.Should().Be(OperationState.Succeeded);
        result.Value.Items.Should().BeEquivalentTo(
            [
                new SeedResourceResult("sqs", "Queue", "seed-orders-queue", true, null),
                new SeedResourceResult("sns", "Topic", "seed-orders-topic", true, null),
            ],
            options => options.WithStrictOrdering());

        published.Should().HaveCount(2);
        published[0].State.Should().Be(OperationState.InProgress);
        published[0].Operation.Should().Be("seed-template");
        published[0].Message.Should().Be("Seeding 'Messaging starter' (2 resource(s)).");
        published[1].State.Should().Be(OperationState.Succeeded);
        published[1].OperationId.Should().Be(published[0].OperationId);
        result.Value.OperationId.Should().Be(published[0].OperationId);

        var entry = appended.Should().ContainSingle().Subject;
        entry.Operation.Should().Be("seed-template");
        entry.State.Should().Be(OperationState.Succeeded);
        entry.Message.Should().Be("Seed 'Messaging starter' completed: 2 succeeded, 0 failed.");
    }

    [Fact]
    public async Task Handle_WhenOneResourceFails_ReturnsFailedOutcomeWithError()
    {
        // Arrange
        _sender.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        Result failure = new Error("boom");
        _sender.Send(Arg.Is<IRequest<Result>>(_ => _ is CreateSnsTopicCommand), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(failure));
        var published = new List<Notification>();
        _publisher
            .PublishAsync(Arg.Do<Notification>(_ => published.Add(_)), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var appended = new List<ActivityEntry>();
        _activityLog.When(_ => _.Append(Arg.Any<ActivityEntry>())).Do(_ => appended.Add(_.Arg<ActivityEntry>()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ApplySeedTemplateCommand("messaging-starter"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SucceededCount.Should().Be(1);
        result.Value.FailedCount.Should().Be(1);
        result.Value.OverallState.Should().Be(OperationState.Failed);
        result.Value.Items.Should().BeEquivalentTo(
            [
                new SeedResourceResult("sqs", "Queue", "seed-orders-queue", true, null),
                new SeedResourceResult("sns", "Topic", "seed-orders-topic", false, "boom"),
            ],
            options => options.WithStrictOrdering());

        published[^1].State.Should().Be(OperationState.Failed);
        var entry = appended.Should().ContainSingle().Subject;
        entry.State.Should().Be(OperationState.Failed);
        entry.Message.Should().Be("Seed 'Messaging starter' completed: 1 succeeded, 1 failed.");
    }

    [Fact]
    public async Task Handle_WhenTemplateUnknown_ReturnsErrorWithoutSideEffects()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ApplySeedTemplateCommand("nope"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("Unknown seed template 'nope'.");

        await _sender.DidNotReceive().Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        _activityLog.DidNotReceive().Append(Arg.Any<ActivityEntry>());
    }
}
