using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for SSM Parameter Store: create a parameter, read its value back,
/// then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SsmHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SsmHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateGetDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var parameterName = $"/itest/ssm/{Guid.NewGuid():N}/config";
        var parameterValue = "integration-happy-path-value";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/ssm-parameter-store/parameters",
            new ParameterCreateRequest(parameterName, "String", parameterValue, "Integration test parameter"),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await client.GetAsync(
            $"/api/services/ssm-parameter-store/parameters/value?name={Uri.EscapeDataString(parameterName)}&reveal=false",
            cancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = await getResponse.Content.ReadFromJsonAsync<ParameterValueResponse>(cancellationToken);
        value.Should().NotBeNull();
        value!.Name.Should().Be(parameterName);
        value.Type.Should().Be("String");

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/ssm-parameter-store/parameters?name={Uri.EscapeDataString(parameterName)}",
            cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
