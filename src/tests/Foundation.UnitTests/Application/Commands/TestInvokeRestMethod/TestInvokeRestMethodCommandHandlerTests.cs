using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGateway;
using Foundation.Application.Commands.TestInvokeRestMethod;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGateway;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.TestInvokeRestMethod;

public class TestInvokeRestMethodCommandHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private static TestInvokeRestMethodCommand BuildCommand()
        => new(
            "api-1",
            "res-2",
            "POST",
            "/orders",
            new Dictionary<string, string> { ["Content-Type"] = "application/json" },
            new Dictionary<string, string> { ["debug"] = "true" },
            "{\"orderId\":\"123\"}",
            new Dictionary<string, string> { ["env"] = "local" });

    private TestInvokeRestMethodCommandHandler CreateSut()
        => new(
            _client,
            _publisher,
            _activityLog,
            NullLogger<TestInvokeRestMethodCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenInvokeSucceeds_ReturnsResponseAndPublishesSuccess()
    {
        // Arrange
        _client
            .TestInvokeMethodAsync(Arg.Any<RestMethodTestInvocationSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestMethodTestInvocationResult>>(
                new RestMethodTestInvocationResult(
                    200,
                    12,
                    new Dictionary<string, string> { ["Content-Type"] = "application/json" },
                    "{\"ok\":true}",
                    "execution log")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StatusCode.Should().Be(200);
        result.Value.LatencyMilliseconds.Should().Be(12);
        result.Value.Body.Should().Be("{\"ok\":true}");
        await _client.Received(1).TestInvokeMethodAsync(
            Arg.Is<RestMethodTestInvocationSpecification>(specification =>
                specification.RestApiId == "api-1"
                && specification.ResourceId == "res-2"
                && specification.HttpMethod == "POST"
                && specification.PathWithQueryString == "/orders"
                && specification.QueryStringParameters.ContainsKey("debug")
                && specification.StageVariables.ContainsKey("env")),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.InProgress),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenInvokeFails_ReturnsErrorAndPublishesFailure()
    {
        // Arrange
        _client
            .TestInvokeMethodAsync(Arg.Any<RestMethodTestInvocationSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestMethodTestInvocationResult>>(new Error("invoke boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("invoke boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
