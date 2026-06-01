using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.CloudFormation;
using Foundation.Application.Commands.UpdateStack;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.CloudFormation;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateStack;

public class UpdateStackCommandHandlerTests
{
    private const string TemplateBody = "{\"Resources\":{}}";

    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static UpdateStackCommand BuildCommand()
        => new("orders-stack", TemplateBody,
            [new StackParameter("Env", "dev")], ["CAPABILITY_IAM"]);

    private UpdateStackCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<UpdateStackCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUpdateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .UpdateStackAsync("orders-stack", TemplateBody,
                Arg.Any<IReadOnlyList<StackParameter>>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("arn:stack/orders-stack"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("arn:stack/orders-stack");
        await _client.Received(1).UpdateStackAsync(
            "orders-stack", TemplateBody,
            Arg.Any<IReadOnlyList<StackParameter>>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
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
    public async Task Handle_WhenUpdateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .UpdateStackAsync(Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<IReadOnlyList<StackParameter>>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("update boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("update boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
