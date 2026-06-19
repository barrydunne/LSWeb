using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class ApiGatewayV2ControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public ApiGatewayV2ControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListApis_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/apigatewayv2/apis", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<HttpApiListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Apis.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetApi_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/apigatewayv2/apis/missing-api",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<HttpApiDetailResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateApi_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new HttpApiCreateRequest(
            "integration-create-api",
            "HTTP",
            "Created by integration test",
            "1.0",
            null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/apigatewayv2/apis", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var payload = await response.Content.ReadFromJsonAsync<HttpApiCreatedResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.ApiId.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task UpdateApi_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new HttpApiUpdateRequest(
            "integration-update-api",
            "HTTP",
            "Updated by integration test",
            "1.0",
            null);

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/services/apigatewayv2/apis/missing-api",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteApi_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/apigatewayv2/apis/missing-api",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ListRoutes_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/apigatewayv2/apis/missing-api/routes",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<HttpRouteListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Routes.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetRoute_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/apigatewayv2/apis/missing-api/routes/missing-route",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task CreateRoute_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new HttpRouteCreateRequest(
            "GET /items",
            null,
            "NONE",
            null,
            null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/apigatewayv2/apis/missing-api/routes",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task UpdateRoute_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new HttpRouteUpdateRequest(
            "GET /items",
            null,
            "NONE",
            null,
            null);

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/services/apigatewayv2/apis/missing-api/routes/missing-route",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteRoute_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/apigatewayv2/apis/missing-api/routes/missing-route",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task TestInvokeRoute_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new HttpRouteTestRequest(
            "$default",
            "GET",
            "/items",
            null,
            null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/apigatewayv2/apis/missing-api/routes/test-invoke",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ListIntegrations_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/apigatewayv2/apis/missing-api/integrations",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<HttpIntegrationListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Integrations.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateIntegration_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new HttpIntegrationCreateRequest(
            "HTTP_PROXY",
            "GET",
            "https://example.test",
            "1.0",
            "Created by integration test");

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/apigatewayv2/apis/missing-api/integrations",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ListAuthorizers_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/apigatewayv2/apis/missing-api/authorizers",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<HttpAuthorizerListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Authorizers.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetAuthorizer_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/apigatewayv2/apis/missing-api/authorizers/missing-authorizer",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task CreateAuthorizer_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new HttpAuthorizerCreateRequest(
            "jwt-authorizer",
            "JWT",
            ["$request.header.Authorization"],
            "https://example.com/issuer",
            ["client1"]);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/apigatewayv2/apis/missing-api/authorizers",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task UpdateAuthorizer_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new HttpAuthorizerUpdateRequest(
            "jwt-authorizer",
            "JWT",
            ["$request.header.Authorization"],
            "https://example.com/issuer",
            ["client1"]);

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/services/apigatewayv2/apis/missing-api/authorizers/missing-authorizer",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteAuthorizer_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/apigatewayv2/apis/missing-api/authorizers/missing-authorizer",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }
}
