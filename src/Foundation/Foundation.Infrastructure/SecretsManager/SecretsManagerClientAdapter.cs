using System.Diagnostics.CodeAnalysis;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.SecretsManager;
using Foundation.Domain.SecretsManager;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.SecretsManager;

/// <summary>
/// Reads Secrets Manager through the resilient AWS gateway so the same code works against LocalStack
/// or real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and
/// converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class SecretsManagerClientAdapter : ISecretsManagerClient
{
    private const string ServiceKey = "secrets-manager";

    private readonly IAwsGateway _gateway;

    public SecretsManagerClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<Secret>>> ListSecretsAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSecretsManagerClient, IReadOnlyList<Secret>>(
            ServiceKey,
            async (client, token) =>
            {
                var secrets = new List<Secret>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListSecretsAsync(
                        new ListSecretsRequest { MaxResults = 100, NextToken = nextToken },
                        token);

                    foreach (var entry in response.SecretList ?? [])
                        secrets.Add(ToSecret(entry));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return secrets;
            },
            cancellationToken);

    public async Task<Result> CreateSecretAsync(
        SecretSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSecretsManagerClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateSecretAsync(
                    new CreateSecretRequest
                    {
                        Name = specification.Name,
                        Description = specification.Description,
                        SecretString = specification.SecretString,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteSecretAsync(string secretId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSecretsManagerClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteSecretAsync(
                    new DeleteSecretRequest { SecretId = secretId, ForceDeleteWithoutRecovery = true },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<SecretValue>> GetSecretValueAsync(string secretId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSecretsManagerClient, SecretValue>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetSecretValueAsync(
                    new GetSecretValueRequest { SecretId = secretId },
                    token);

                return new SecretValue(
                    response.Name ?? string.Empty,
                    response.ARN ?? string.Empty,
                    string.IsNullOrEmpty(response.VersionId) ? null : response.VersionId,
                    response.SecretString ?? string.Empty);
            },
            cancellationToken);

    public async Task<Result> PutSecretValueAsync(
        SecretValueSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSecretsManagerClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutSecretValueAsync(
                    new PutSecretValueRequest
                    {
                        SecretId = specification.SecretId,
                        SecretString = specification.SecretString,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<SecretVersionList>> ListSecretVersionsAsync(
        string secretId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSecretsManagerClient, SecretVersionList>(
            ServiceKey,
            async (client, token) =>
            {
                var versions = new List<SecretVersion>();
                string? name = null;
                string? arn = null;
                string? nextToken = null;

                do
                {
                    var response = await client.ListSecretVersionIdsAsync(
                        new ListSecretVersionIdsRequest
                        {
                            SecretId = secretId,
                            MaxResults = 100,
                            NextToken = nextToken,
                        },
                        token);

                    name ??= response.Name;
                    arn ??= response.ARN;

                    foreach (var entry in response.Versions ?? [])
                        versions.Add(ToSecretVersion(entry));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return new SecretVersionList(name ?? secretId, arn ?? string.Empty, versions);
            },
            cancellationToken);

    private static Secret ToSecret(SecretListEntry entry)
        => new(
            entry.Name ?? string.Empty,
            entry.ARN ?? string.Empty,
            string.IsNullOrEmpty(entry.Description) ? null : entry.Description,
            entry.CreatedDate is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(entry.CreatedDate.Value, DateTimeKind.Utc)),
            entry.LastChangedDate is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(entry.LastChangedDate.Value, DateTimeKind.Utc)));

    private static SecretVersion ToSecretVersion(SecretVersionsListEntry entry)
        => new(
            entry.VersionId ?? string.Empty,
            entry.VersionStages ?? [],
            entry.CreatedDate is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(entry.CreatedDate.Value, DateTimeKind.Utc)),
            entry.LastAccessedDate is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(entry.LastAccessedDate.Value, DateTimeKind.Utc)));
}
