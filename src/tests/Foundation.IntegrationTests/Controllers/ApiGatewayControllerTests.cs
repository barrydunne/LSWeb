using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class ApiGatewayControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public ApiGatewayControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListRestApis_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/apigateway/restapis", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<RestApiListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.RestApis.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetRestCors_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/apigateway/restapis/unknown-api/resources/unknown-resource/cors",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ConfigureRestCors_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new RestCorsConfigureRequest(
            ["*"],
            ["GET", "POST"],
            ["Content-Type"]);

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/services/apigateway/restapis/unknown-api/resources/unknown-resource/cors",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task CreateRestTokenAuthorizer_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new RestTokenAuthorizerCreateRequest(
            "jwt-authorizer",
            "https://issuer.example.com",
            "api://default",
            "method.request.header.Authorization",
            "arn:aws:apigateway:eu-west-1:lambda:path/2015-03-31/functions/authorizer/invocations");

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/apigateway/restapis/unknown-api/authorizers/token",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }
}
