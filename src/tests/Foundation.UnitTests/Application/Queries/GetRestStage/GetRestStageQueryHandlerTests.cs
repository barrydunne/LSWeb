using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Application.Queries.GetRestStage;
using Foundation.Domain.ApiGateway;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetRestStage;

public class GetRestStageQueryHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private GetRestStageQueryHandler CreateSut()
        => new(_client, NullLogger<GetRestStageQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsStage()
    {
        // Arrange
        var detail = new RestStageDetail(
            "dev",
            "deployment-1",
            "Development stage",
            true,
            new Dictionary<string, string> { ["key"] = "value" },
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch.AddDays(1));
        _client
            .GetStageAsync("api-1", "dev", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestStageDetail>>(detail));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetRestStageQuery("api-1", "dev"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Stage.StageName.Should().Be("dev");
        result.Value.Stage.DeploymentId.Should().Be("deployment-1");
        result.Value.Stage.Description.Should().Be("Development stage");
        result.Value.Stage.CacheClusterEnabled.Should().BeTrue();
        result.Value.Stage.Variables.Should().ContainKey("key").WhoseValue.Should().Be("value");
        result.Value.Stage.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
        result.Value.Stage.LastUpdatedDate.Should().Be(DateTimeOffset.UnixEpoch.AddDays(1));
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetStageAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestStageDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetRestStageQuery("api-1", "dev"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
