using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class CognitoControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public CognitoControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListUserPools_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/cognito/user-pools", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<UserPoolListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.UserPools.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetUserPool_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/cognito/user-pools/eu-west-1_missing",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<UserPoolDetailResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateUserPool_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new UserPoolCreateRequest(
            "integration-create-pool",
            "OFF",
            ["email"],
            ["email"]);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/cognito/user-pools", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteUserPool_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/cognito/user-pools/eu-west-1_missing",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ListUserPoolClients_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/cognito/user-pools/eu-west-1_missing/clients",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<UserPoolClientListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Clients.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetUserPoolClient_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/cognito/user-pools/eu-west-1_missing/clients/missing-client",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<UserPoolClientDetailResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateUserPoolClient_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new UserPoolClientCreateRequest(
            "integration-create-client",
            false,
            ["ALLOW_USER_SRP_AUTH"],
            ["code"],
            ["openid"],
            ["https://app/callback"],
            true);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/cognito/user-pools/eu-west-1_missing/clients",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task UpdateUserPoolClient_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new UserPoolClientUpdateRequest(
            "integration-update-client",
            ["ALLOW_USER_SRP_AUTH"],
            ["code"],
            ["openid"],
            ["https://app/callback"],
            true);

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/services/cognito/user-pools/eu-west-1_missing/clients/missing-client",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteUserPoolClient_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/cognito/user-pools/eu-west-1_missing/clients/missing-client",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }
}
