using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for Secrets Manager: create a secret, confirm it is listed and its value
/// can be read back, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SecretsManagerHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SecretsManagerHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListGetDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var secretName = $"itest-secret-{Guid.NewGuid():N}";
        var secretValue = "integration-happy-path-secret";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/secrets-manager/secrets",
            new SecretCreateRequest(secretName, "Integration test secret", secretValue),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<SecretListResponse>(
            "/api/services/secrets-manager/secrets", cancellationToken);
        list.Should().NotBeNull();
        list!.Secrets.Should().ContainSingle(secret => secret.Name == secretName);

        var getResponse = await client.GetAsync(
            $"/api/services/secrets-manager/secrets/{Uri.EscapeDataString(secretName)}/value?reveal=true",
            cancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = await getResponse.Content.ReadFromJsonAsync<SecretValueResponse>(cancellationToken);
        value.Should().NotBeNull();
        value!.Value.Should().Be(secretValue);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/secrets-manager/secrets/{Uri.EscapeDataString(secretName)}",
            cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
