using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Application.Queries.ListRestDeployments;
using Foundation.Domain.ApiGateway;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListRestDeployments;

public class ListRestDeploymentsQueryHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private ListRestDeploymentsQueryHandler CreateSut()
        => new(_client, NullLogger<ListRestDeploymentsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsDeployments()
    {
        // Arrange
        IReadOnlyList<RestDeploymentSummary> deployments =
        [
            new("deployment-1", "Initial deployment", DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListDeploymentsAsync("api-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(deployments)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRestDeploymentsQuery("api-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deployment = result.Value.Deployments.Should().ContainSingle().Subject;
        deployment.Id.Should().Be("deployment-1");
        deployment.Description.Should().Be("Initial deployment");
        deployment.CreatedDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListDeploymentsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<RestDeploymentSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRestDeploymentsQuery("api-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
