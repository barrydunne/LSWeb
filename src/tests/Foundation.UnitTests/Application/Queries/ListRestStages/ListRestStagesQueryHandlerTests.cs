using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Application.Queries.ListRestStages;
using Foundation.Domain.ApiGateway;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListRestStages;

public class ListRestStagesQueryHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private ListRestStagesQueryHandler CreateSut()
        => new(_client, NullLogger<ListRestStagesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsStages()
    {
        // Arrange
        IReadOnlyList<RestStageSummary> stages =
        [
            new("dev", "deployment-1", DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListStagesAsync("api-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stages)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRestStagesQuery("api-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stage = result.Value.Stages.Should().ContainSingle().Subject;
        stage.StageName.Should().Be("dev");
        stage.DeploymentId.Should().Be("deployment-1");
        stage.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListStagesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<RestStageSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRestStagesQuery("api-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
