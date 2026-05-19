using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.UpdateLambdaFunction;
using Foundation.Application.Lambda;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Lambda;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateLambdaFunction;

public class UpdateLambdaFunctionCommandHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private UpdateLambdaFunctionCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<UpdateLambdaFunctionCommandHandler>.Instance);

    private static UpdateLambdaFunctionCommand Command(string? zip)
        => new("orders", "python3.12", "index.handler", "arn:role", "Order processor", 256, 30, zip);

    [Fact]
    public async Task Handle_WhenConfigurationUpdateSucceedsWithoutCode_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .UpdateConfigurationAsync(Arg.Any<LambdaConfigurationUpdateSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(null), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).UpdateConfigurationAsync(
            Arg.Is<LambdaConfigurationUpdateSpec>(spec =>
                spec.FunctionName == "orders"
                && spec.Runtime == "python3.12"
                && spec.Handler == "index.handler"
                && spec.Role == "arn:role"
                && spec.Description == "Order processor"
                && spec.MemorySize == 256
                && spec.Timeout == 30),
            Arg.Any<CancellationToken>());
        await _client.DidNotReceive().UpdateCodeAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenCodeSupplied_UpdatesCodeAndPublishesSuccess()
    {
        // Arrange
        _client
            .UpdateConfigurationAsync(Arg.Any<LambdaConfigurationUpdateSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        _client
            .UpdateCodeAsync("orders", "QkFTRTY0", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command("QkFTRTY0"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).UpdateCodeAsync("orders", "QkFTRTY0", Arg.Any<CancellationToken>());
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenConfigurationUpdateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .UpdateConfigurationAsync(Arg.Any<LambdaConfigurationUpdateSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("config boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command("QkFTRTY0"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("config boom");
        await _client.DidNotReceive().UpdateCodeAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenCodeUpdateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .UpdateConfigurationAsync(Arg.Any<LambdaConfigurationUpdateSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        _client
            .UpdateCodeAsync("orders", "QkFTRTY0", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("code boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command("QkFTRTY0"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("code boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
