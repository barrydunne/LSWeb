using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Queries.ListHttpStages;
using Foundation.Domain.ApiGatewayV2;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListHttpStages;

public class ListHttpStagesQueryHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();

    private ListHttpStagesQueryHandler CreateSut()
        => new(_client, NullLogger<ListHttpStagesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsStages()
    {
        // Arrange
        IReadOnlyList<HttpStageSummary> stages =
        [
            new("dev", true, "deploy1", DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListStagesAsync("abc123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stages)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHttpStagesQuery("abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stage = result.Value.Stages.Should().ContainSingle().Subject;
        stage.StageName.Should().Be("dev");
        stage.AutoDeploy.Should().BeTrue();
        stage.DeploymentId.Should().Be("deploy1");
        stage.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListStagesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<HttpStageSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHttpStagesQuery("abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
