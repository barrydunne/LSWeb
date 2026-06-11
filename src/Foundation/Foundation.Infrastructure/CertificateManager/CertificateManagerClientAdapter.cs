using System.Diagnostics.CodeAnalysis;
using System.Text;
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

    public Task<Result<string>> ImportCertificateAsync(
        CertificateImportSpecification specification,
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCertificateManagerClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new ImportCertificateRequest
                {
                    Certificate = ToStream(specification.Certificate),
                    PrivateKey = ToStream(specification.PrivateKey),
                };

                if (!string.IsNullOrEmpty(specification.CertificateChain))
                    request.CertificateChain = ToStream(specification.CertificateChain);

                var response = await client.ImportCertificateAsync(request, token);
                return response.CertificateArn ?? string.Empty;
            },
            cancellationToken);

    public Task<Result<string>> RequestCertificateAsync(
        CertificateRequestSpecification specification,
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCertificateManagerClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new RequestCertificateRequest
                {
                    DomainName = specification.DomainName,
                    ValidationMethod = ValidationMethod.FindValue(specification.ValidationMethod),
                };

                if (specification.SubjectAlternativeNames.Count > 0)
                    request.SubjectAlternativeNames = [.. specification.SubjectAlternativeNames];

                var response = await client.RequestCertificateAsync(request, token);
                return response.CertificateArn ?? string.Empty;
            },
            cancellationToken);

    private static MemoryStream ToStream(string value)
        => new(Encoding.UTF8.GetBytes(value));

    private static Certificate ToCertificate(CertificateSummary summary)
        => new(
            summary.CertificateArn ?? string.Empty,
            summary.DomainName ?? string.Empty,
            summary.Status?.Value ?? string.Empty,
            string.IsNullOrWhiteSpace(summary.Type?.Value) ? null : summary.Type.Value);
}
