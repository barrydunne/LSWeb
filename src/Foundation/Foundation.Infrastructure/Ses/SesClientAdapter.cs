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

    public Task<Result<SesIdentityDetail>> GetIdentityAsync(
        string identity,
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSimpleEmailServiceClient, SesIdentityDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetIdentityVerificationAttributesAsync(
                    new GetIdentityVerificationAttributesRequest { Identities = [identity] },
                    token);

                var status = response.VerificationAttributes is not null
                    && response.VerificationAttributes.TryGetValue(identity, out var attributes)
                    ? attributes.VerificationStatus?.Value ?? "Unknown"
                    : "NotStarted";

                return new SesIdentityDetail(
                    identity,
                    identity.Contains('@', StringComparison.Ordinal) ? "EmailAddress" : "Domain",
                    status);
            },
            cancellationToken);

    public Task<Result> VerifyEmailIdentityAsync(
        string emailAddress,
        CancellationToken cancellationToken)
        => RunVoidAsync(
            (client, token) => client.VerifyEmailIdentityAsync(
                new VerifyEmailIdentityRequest { EmailAddress = emailAddress }, token),
            cancellationToken);

    public Task<Result> DeleteIdentityAsync(
        string identity,
        CancellationToken cancellationToken)
        => RunVoidAsync(
            (client, token) => client.DeleteIdentityAsync(
                new DeleteIdentityRequest { Identity = identity }, token),
            cancellationToken);

    public Task<Result> VerifyDomainIdentityAsync(
        string domain,
        CancellationToken cancellationToken)
        => RunVoidAsync(
            (client, token) => client.VerifyDomainIdentityAsync(
                new VerifyDomainIdentityRequest { Domain = domain }, token),
            cancellationToken);

    public Task<Result> EnableDomainDkimAsync(
        string domain,
        CancellationToken cancellationToken)
        => RunVoidAsync(
            (client, token) => client.VerifyDomainDkimAsync(
                new VerifyDomainDkimRequest { Domain = domain }, token),
            cancellationToken);

    public Task<Result<SesDomainSetup>> GetDomainSetupAsync(
        string domain,
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSimpleEmailServiceClient, SesDomainSetup>(
            ServiceKey,
            async (client, token) =>
            {
                var verification = await client.GetIdentityVerificationAttributesAsync(
                    new GetIdentityVerificationAttributesRequest { Identities = [domain] }, token);

                var verificationStatus = "NotStarted";
                var verificationToken = string.Empty;
                if (verification.VerificationAttributes is not null
                    && verification.VerificationAttributes.TryGetValue(domain, out var attributes))
                {
                    verificationStatus = attributes.VerificationStatus?.Value ?? "Unknown";
                    verificationToken = attributes.VerificationToken ?? string.Empty;
                }

                var dkim = await client.GetIdentityDkimAttributesAsync(
                    new GetIdentityDkimAttributesRequest { Identities = [domain] }, token);

                var dkimStatus = "NotStarted";
                IReadOnlyList<string> dkimTokens = [];
                if (dkim.DkimAttributes is not null
                    && dkim.DkimAttributes.TryGetValue(domain, out var dkimAttributes))
                {
                    dkimStatus = dkimAttributes.DkimVerificationStatus?.Value ?? "Unknown";
                    dkimTokens = dkimAttributes.DkimTokens ?? [];
                }

                return new SesDomainSetup(
                    domain, verificationStatus, verificationToken, dkimStatus, dkimTokens);
            },
            cancellationToken);

    private async Task<Result> RunVoidAsync(
        Func<AmazonSimpleEmailServiceClient, CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSimpleEmailServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await action(client, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

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
