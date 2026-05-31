using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class StepFunctionsControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public StepFunctionsControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListStateMachines_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/step-functions/state-machines", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<StateMachineListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.StateMachines.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetStateMachine_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/step-functions/state-machine?arn=arn:aws:states:eu-west-1:000000000000:stateMachine:integration-missing",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetExecutionHistory_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/step-functions/execution-history?arn=arn:aws:states:eu-west-1:000000000000:execution:integration-missing:run-1",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<ExecutionHistoryResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Events.Should().NotBeNull();
        }
    }
}
