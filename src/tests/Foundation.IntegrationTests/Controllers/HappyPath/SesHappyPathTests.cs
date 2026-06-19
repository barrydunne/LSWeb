using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for SES: verify an email identity, confirm it appears in the listing,
/// then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SesHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SesHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var emailAddress = $"itest-{Guid.NewGuid():N}@example.com";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/ses/identities",
            new SesVerifyEmailRequest(emailAddress),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<SesIdentityListResponse>(
            "/api/services/ses/identities",
            cancellationToken);
        list.Should().NotBeNull();
        list!.Identities.Should().ContainSingle(identity => identity.Identity == emailAddress);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/ses/identities/{Uri.EscapeDataString(emailAddress)}",
            cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
