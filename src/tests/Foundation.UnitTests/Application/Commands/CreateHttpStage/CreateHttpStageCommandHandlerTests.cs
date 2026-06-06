using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Commands.CreateHttpStage;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGatewayV2;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateHttpStage;

public class CreateHttpStageCommandHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static CreateHttpStageCommand BuildCommand()
        => new(
            "abc123",
            "dev",
            true,
            "Development stage",
            100,
            50.0,
            new Dictionary<string, string> { ["color"] = "blue" });

    private CreateHttpStageCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateHttpStageCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .CreateStageAsync(Arg.Any<HttpStageSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("dev"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("dev");
        await _client.Received(1).CreateStageAsync(
            Arg.Is<HttpStageSpecification>(specification =>
                specification.ApiId == "abc123"
                && specification.StageName == "dev"
                && specification.AutoDeploy),
            Arg.Any<CancellationToken>());
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
    public async Task Handle_WhenCreateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .CreateStageAsync(Arg.Any<HttpStageSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("create boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("create boom");
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
            .CreateStageAsync(Arg.Any<HttpStageSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("prod"));
        var command = new CreateHttpStageCommand(
            "api9",
            "prod",
            false,
            "Production",
            200,
            75.5,
            new Dictionary<string, string> { ["env"] = "prod" });
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateStageAsync(
            Arg.Is<HttpStageSpecification>(specification =>
                specification.ApiId == "api9"
                && specification.StageName == "prod"
                && !specification.AutoDeploy
                && specification.Description == "Production"
                && specification.DefaultRouteThrottlingBurstLimit == 200
                && specification.DefaultRouteThrottlingRateLimit == 75.5
                && specification.StageVariables["env"] == "prod"),
            Arg.Any<CancellationToken>());
    }
}
