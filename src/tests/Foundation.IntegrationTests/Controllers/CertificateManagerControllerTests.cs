using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class CertificateManagerControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public CertificateManagerControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListCertificates_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/acm/certificates", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<CertificateListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Certificates.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ImportCertificate_WhenSuppliedValidMaterial_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var (certificatePem, privateKeyPem) = CreateSelfSignedMaterial();
        var request = new CertificateImportRequest(certificatePem, privateKeyPem, null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/acm/certificates/import", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var payload = await response.Content.ReadFromJsonAsync<CertificateImportResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Arn.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task RequestCertificate_WhenSuppliedDomain_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new CertificateRequestRequest(
            "integration.test", "DNS", ["www.integration.test"]);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/acm/certificates", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var payload = await response.Content.ReadFromJsonAsync<CertificateRequestResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Arn.Should().NotBeNullOrWhiteSpace();
        }
    }

    private static (string CertificatePem, string PrivateKeyPem) CreateSelfSignedMaterial()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=integration.test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        return (certificate.ExportCertificatePem(), rsa.ExportPkcs8PrivateKeyPem());
    }
}
