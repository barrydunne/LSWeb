using System.Diagnostics.CodeAnalysis;
using Amazon.CertificateManager;
using Amazon.CertificateManager.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CertificateManager;
using Foundation.Domain.CertificateManager;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.CertificateManager;

/// <summary>
/// Reads ACM through the resilient AWS gateway so the same code works against LocalStack or real
/// AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and converts
/// failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class CertificateManagerClientAdapter : ICertificateManagerClient
{
    private const string ServiceKey = "acm";

    private readonly IAwsGateway _gateway;

    public CertificateManagerClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<Certificate>>> ListCertificatesAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCertificateManagerClient, IReadOnlyList<Certificate>>(
            ServiceKey,
            async (client, token) =>
            {
                var certificates = new List<Certificate>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListCertificatesAsync(
                        new ListCertificatesRequest { NextToken = nextToken },
                        token);

                    foreach (var summary in response.CertificateSummaryList ?? [])
                        certificates.Add(ToCertificate(summary));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return certificates;
            },
            cancellationToken);

    private static Certificate ToCertificate(CertificateSummary summary)
        => new(
            summary.CertificateArn ?? string.Empty,
            summary.DomainName ?? string.Empty,
            summary.Status?.Value ?? string.Empty,
            string.IsNullOrWhiteSpace(summary.Type?.Value) ? null : summary.Type.Value);
}
