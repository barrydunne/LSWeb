using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.DeletePolicy;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Iam;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeletePolicy;

public class DeletePolicyCommandHandlerTests
{
    private const string Arn = "arn:aws:iam::000000000000:policy/deploy-policy";

    private readonly IIamClient _client = Substitute.For<IIamClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static Result<T> Ok<T>(T value) => value;

    private static IamPolicyDetail Detail(int attachmentCount)
        => new(
            "deploy-policy",
            Arn,
            "ANPA1",
            "/",
            "v1",
            attachmentCount,
            true,
            null,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            "{\"Version\":\"2012-10-17\",\"Statement\":[]}",
            [],
            []);

    private DeletePolicyCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<DeletePolicyCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenNotAttached_DeletesAndRefreshesSearch()
    {
        // Arrange
        _client.GetPolicyAsync(Arn, Arg.Any<CancellationToken>()).Returns(Ok(Detail(0)));
        _client
            .DeletePolicyAsync(Arn, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new DeletePolicyCommand(Arn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).DeletePolicyAsync(Arn, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenAttached_ReturnsErrorAndDoesNotDelete()
    {
        // Arrange
        _client.GetPolicyAsync(Arn, Arg.Any<CancellationToken>()).Returns(Ok(Detail(3)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new DeletePolicyCommand(Arn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Contain("attached to 3");
        await _client.DidNotReceive().DeletePolicyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenLookupFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .GetPolicyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Error("lookup boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new DeletePolicyCommand(Arn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("lookup boom");
        await _client.DidNotReceive().DeletePolicyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _searchRefresh.DidNotReceive().RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenDeleteFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client.GetPolicyAsync(Arn, Arg.Any<CancellationToken>()).Returns(Ok(Detail(0)));
        _client
            .DeletePolicyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("delete boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new DeletePolicyCommand(Arn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("delete boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
