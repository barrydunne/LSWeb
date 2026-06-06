using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Queries.GetHttpStage;
using Foundation.Domain.ApiGatewayV2;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetHttpStage;

public class GetHttpStageQueryHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();

    private GetHttpStageQueryHandler CreateSut()
        => new(_client, NullLogger<GetHttpStageQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsStage()
    {
        // Arrange
        var detail = new HttpStageDetail(
            "dev",
            true,
            "deploy1",
            "Development stage",
            100,
            50.0,
            new Dictionary<string, string> { ["color"] = "blue" },
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch.AddDays(1));
        _client
            .GetStageAsync("abc123", "dev", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpStageDetail>>(detail));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetHttpStageQuery("abc123", "dev"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stage = result.Value.Stage;
        stage.StageName.Should().Be("dev");
        stage.AutoDeploy.Should().BeTrue();
        stage.DeploymentId.Should().Be("deploy1");
        stage.Description.Should().Be("Development stage");
        stage.DefaultRouteThrottlingBurstLimit.Should().Be(100);
        stage.DefaultRouteThrottlingRateLimit.Should().Be(50.0);
        stage.StageVariables.Should().ContainKey("color").WhoseValue.Should().Be("blue");
        stage.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
        stage.LastUpdatedDate.Should().Be(DateTimeOffset.UnixEpoch.AddDays(1));
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetStageAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpStageDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetHttpStageQuery("abc123", "dev"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
