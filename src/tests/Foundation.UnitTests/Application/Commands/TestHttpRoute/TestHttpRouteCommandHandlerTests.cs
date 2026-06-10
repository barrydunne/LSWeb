using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Commands.TestHttpRoute;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGatewayV2;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.TestHttpRoute;

public class TestHttpRouteCommandHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();
    private readonly IHttpApiRouteInvoker _invoker = Substitute.For<IHttpApiRouteInvoker>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private static HttpApiDetail BuildApiDetail(string? apiEndpoint)
        => new(
            "api-1",
            "shop",
            "HTTP",
            apiEndpoint,
            "Shop API",
            "1.0",
            "$request.method $request.path",
            null,
            DateTimeOffset.UtcNow);

    private static TestHttpRouteCommand BuildCommand(
        string stage = "$default",
        string method = "GET",
        string path = "/orders",
        string? token = "token-123",
        string? body = null)
        => new("api-1", stage, method, path, token, body);

    private TestHttpRouteCommandHandler CreateSut()
        => new(
            _client,
            _invoker,
            _publisher,
            _activityLog,
            NullLogger<TestHttpRouteCommandHandler>.Instance);

    private void SetupApi(string? apiEndpoint)
        => _client
            .GetApiAsync("api-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpApiDetail>>(BuildApiDetail(apiEndpoint)));

    private void SetupInvocation(Result<HttpRouteInvocationResult> result)
        => _invoker
            .InvokeAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

    [Fact]
    public async Task Handle_WhenInvocationAuthorized_ReturnsResultAndPublishesSuccess()
    {
        // Arrange
        SetupApi("https://api-1.execute-api.localhost.localstack.cloud:4566");
        SetupInvocation(new HttpRouteInvocationResult(
            200,
            true,
            8,
            new Dictionary<string, string> { ["Content-Type"] = "application/json" },
            "{\"ok\":true}"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StatusCode.Should().Be(200);
        result.Value.Authorized.Should().BeTrue();
        result.Value.LatencyMilliseconds.Should().Be(8);
        result.Value.Body.Should().Be("{\"ok\":true}");
        result.Value.Headers.Should().ContainKey("Content-Type");
        await _invoker.Received(1).InvokeAsync(
            "https://api-1.execute-api.localhost.localstack.cloud:4566/orders",
            "GET",
            "token-123",
            null,
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
    public async Task Handle_WhenInvocationUnauthorized_ReturnsResultWithAuthorizedFalse()
    {
        // Arrange
        SetupApi("https://api-1.example/");
        SetupInvocation(new HttpRouteInvocationResult(
            401,
            false,
            3,
            new Dictionary<string, string>(),
            "Unauthorized"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            BuildCommand(token: null), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StatusCode.Should().Be(401);
        result.Value.Authorized.Should().BeFalse();
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStageIsNamed_BuildsUriWithStageSegment()
    {
        // Arrange
        SetupApi("https://api-1.example");
        SetupInvocation(new HttpRouteInvocationResult(
            200, true, 1, new Dictionary<string, string>(), string.Empty));
        var sut = CreateSut();

        // Act
        await sut.Handle(
            BuildCommand(stage: "/prod/", path: "items"), TestContext.Current.CancellationToken);

        // Assert
        await _invoker.Received(1).InvokeAsync(
            "https://api-1.example/prod/items",
            "GET",
            "token-123",
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPathIsBlank_BuildsUriWithRootPath()
    {
        // Arrange
        SetupApi("https://api-1.example");
        SetupInvocation(new HttpRouteInvocationResult(
            200, true, 1, new Dictionary<string, string>(), string.Empty));
        var sut = CreateSut();

        // Act
        await sut.Handle(
            BuildCommand(stage: "   ", path: "   "), TestContext.Current.CancellationToken);

        // Assert
        await _invoker.Received(1).InvokeAsync(
            "https://api-1.example/",
            "GET",
            "token-123",
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenApiCannotBeRead_ReturnsErrorAndPublishesFailure()
    {
        // Arrange
        _client
            .GetApiAsync("api-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpApiDetail>>(new Error("api boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("api boom");
        await _invoker.DidNotReceive().InvokeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenApiHasNoEndpoint_ReturnsErrorAndPublishesFailure(string? apiEndpoint)
    {
        // Arrange
        SetupApi(apiEndpoint);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Contain("does not expose an invoke endpoint");
        await _invoker.DidNotReceive().InvokeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInvocationFails_ReturnsErrorAndPublishesFailure()
    {
        // Arrange
        SetupApi("https://api-1.example");
        SetupInvocation(new Error("invoke boom"));
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
