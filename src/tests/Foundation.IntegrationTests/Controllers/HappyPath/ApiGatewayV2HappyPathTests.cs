using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for API Gateway v2 (HTTP APIs). API Gateway v2 is a LocalStack Pro feature,
/// so the test is skipped on the community edition. Creates an HTTP API, confirms it is retrievable,
/// then deletes it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class ApiGatewayV2HappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public ApiGatewayV2HappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateGetDelete_RoundTripsSuccessfully()
    {
        _fixture.SkipIfLocalStackNotPro();

        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var apiName = $"itest-apigwv2-{Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/apigatewayv2/apis",
            new HttpApiCreateRequest(
                apiName,
                "HTTP",
                "Created by integration test",
                "1.0",
                null),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<HttpApiCreatedResponse>(cancellationToken);
        created.Should().NotBeNull();
        var apiId = created!.ApiId;
        apiId.Should().NotBeNullOrWhiteSpace();

        var getResponse = await client.GetAsync(
            $"/api/services/apigatewayv2/apis/{apiId}", cancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await getResponse.Content.ReadFromJsonAsync<HttpApiDetailResponse>(cancellationToken);
        detail.Should().NotBeNull();
        detail!.Name.Should().Be(apiName);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/apigatewayv2/apis/{apiId}", cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
