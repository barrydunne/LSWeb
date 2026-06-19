using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for Step Functions: create a minimal state machine, confirm it is listed
/// and retrievable, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class StepFunctionsHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public StepFunctionsHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateGetDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var stateMachineName = $"itest-sfn-{Guid.NewGuid():N}";
        var definition = """{"StartAt":"Pass","States":{"Pass":{"Type":"Pass","End":true}}}""";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/step-functions/state-machines",
            new CreateStateMachineRequest(
                stateMachineName,
                definition,
                "arn:aws:iam::000000000000:role/itest-sfn-role",
                "STANDARD"),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateStateMachineResponse>(cancellationToken);
        created.Should().NotBeNull();
        var stateMachineArn = created!.StateMachineArn;
        stateMachineArn.Should().NotBeNullOrWhiteSpace();

        var getResponse = await client.GetAsync(
            $"/api/services/step-functions/state-machine?arn={Uri.EscapeDataString(stateMachineArn)}", cancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await getResponse.Content.ReadFromJsonAsync<StateMachineDetailResponse>(cancellationToken);
        detail.Should().NotBeNull();
        detail!.StateMachineArn.Should().Be(stateMachineArn);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/step-functions/state-machine?arn={Uri.EscapeDataString(stateMachineArn)}", cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
