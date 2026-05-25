using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.PutSecretValue;
using Foundation.Application.Search;
using Foundation.Application.SecretsManager;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.SecretsManager;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutSecretValue;

public class PutSecretValueCommandHandlerTests
{
    private readonly ISecretsManagerClient _client = Substitute.For<ISecretsManagerClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static PutSecretValueCommand BuildCommand()
        => new("db-password", "new-value");

    private PutSecretValueCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<PutSecretValueCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenPutSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .PutSecretValueAsync(Arg.Any<SecretValueSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
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
        // Arrange
        _client
            .PutSecretValueAsync(Arg.Any<SecretValueSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("put boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
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
        // Arrange
        _client
            .PutSecretValueAsync(Arg.Any<SecretValueSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var command = new PutSecretValueCommand("db-password", "new-value");
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).PutSecretValueAsync(
            Arg.Is<SecretValueSpecification>(spec =>
                spec.SecretId == "db-password"
                && spec.SecretString == "new-value"),
            Arg.Any<CancellationToken>());
    }
}
