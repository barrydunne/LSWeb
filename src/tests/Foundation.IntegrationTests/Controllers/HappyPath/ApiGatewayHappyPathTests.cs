using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for API Gateway (REST): create a REST API, confirm it is listed and
/// retrievable, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class ApiGatewayHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public ApiGatewayHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateGetDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var apiName = $"itest-apigw-{Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/apigateway/restapis",
            new RestApiCreateRequest(
                apiName,
                "Created by integration test",
                "1.0",
                null,
                null),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<RestApiCreatedResponse>(cancellationToken);
        created.Should().NotBeNull();
        var restApiId = created!.Id;
        restApiId.Should().NotBeNullOrWhiteSpace();

        var getResponse = await client.GetAsync(
            $"/api/services/apigateway/restapis/{restApiId}", cancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await getResponse.Content.ReadFromJsonAsync<RestApiDetailResponse>(cancellationToken);
        detail.Should().NotBeNull();
        detail!.Name.Should().Be(apiName);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/apigateway/restapis/{restApiId}", cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
