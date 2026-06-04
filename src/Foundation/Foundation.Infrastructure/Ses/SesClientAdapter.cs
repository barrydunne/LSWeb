using System.Diagnostics.CodeAnalysis;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Ses;
using Foundation.Domain.Ses;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.Ses;

/// <summary>
/// Reads SES through the resilient AWS gateway so the same code works against LocalStack or real
/// AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and converts
/// failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class SesClientAdapter : ISesClient
{
    private const string ServiceKey = "ses";
    private const int VerificationBatchSize = 100;

    private readonly IAwsGateway _gateway;

    public SesClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<SesIdentity>>> ListIdentitiesAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSimpleEmailServiceClient, IReadOnlyList<SesIdentity>>(
            ServiceKey,
            async (client, token) =>
            {
                var names = new List<string>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListIdentitiesAsync(
                        new ListIdentitiesRequest { NextToken = nextToken },
                        token);

                    names.AddRange(response.Identities ?? []);
                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                var statuses = await GetVerificationStatusesAsync(client, names, token);

                return names
                    .Select(name => new SesIdentity(
                        name,
                        name.Contains('@', StringComparison.Ordinal) ? "EmailAddress" : "Domain",
                        statuses.TryGetValue(name, out var status) ? status : "Unknown"))
                    .ToList();
            },
            cancellationToken);

    private static async Task<Dictionary<string, string>> GetVerificationStatusesAsync(
        AmazonSimpleEmailServiceClient client,
        List<string> names,
        CancellationToken cancellationToken)
    {
        var statuses = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var offset = 0; offset < names.Count; offset += VerificationBatchSize)
        {
            var batch = names
                .Skip(offset)
                .Take(VerificationBatchSize)
                .ToList();

            var response = await client.GetIdentityVerificationAttributesAsync(
                new GetIdentityVerificationAttributesRequest { Identities = batch },
                cancellationToken);

            foreach (var (identity, attributes) in response.VerificationAttributes ?? [])
                statuses[identity] = attributes.VerificationStatus?.Value ?? "Unknown";
        }

        return statuses;
    }
}
