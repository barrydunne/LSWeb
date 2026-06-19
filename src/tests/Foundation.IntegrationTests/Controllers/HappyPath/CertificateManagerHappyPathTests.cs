using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for Certificate Manager: request a certificate and confirm it appears in
/// the listing. ACM does not expose a delete endpoint, so the certificate is left for the ephemeral
/// container teardown to discard.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class CertificateManagerHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public CertificateManagerHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task RequestThenList_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var domainName = $"itest-acm-{Guid.NewGuid():N}.example.com";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/acm/certificates",
            new CertificateRequestRequest(domainName, "DNS", null),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CertificateRequestResponse>(cancellationToken);
        created.Should().NotBeNull();
        created!.Arn.Should().Contain("arn:aws:acm:");

        var list = await client.GetFromJsonAsync<CertificateListResponse>(
            "/api/services/acm/certificates", cancellationToken);
        list.Should().NotBeNull();
        list!.Certificates.Should().Contain(certificate => certificate.Arn == created.Arn);
    }
}
