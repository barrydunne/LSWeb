using System.Diagnostics.CodeAnalysis;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Ssm;
using Foundation.Domain.Ssm;
using Foundation.Infrastructure.Aws;
using AwsParameter = Amazon.SimpleSystemsManagement.Model.Parameter;
using Parameter = Foundation.Domain.Ssm.Parameter;

namespace Foundation.Infrastructure.Ssm;

/// <summary>
/// Reads and writes SSM Parameter Store through the resilient AWS gateway so the same code works
/// against LocalStack or real AWS. All access flows through <see cref="IAwsGateway"/>, which records
/// capability and converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class SsmClientAdapter : ISsmClient
{
    private const string ServiceKey = "ssm-parameter-store";

    private readonly IAwsGateway _gateway;

    public SsmClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<Parameter>>> GetParametersByPathAsync(
        string path, bool recursive, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSimpleSystemsManagementClient, IReadOnlyList<Parameter>>(
            ServiceKey,
            async (client, token) =>
            {
                var parameters = new List<Parameter>();
                string? nextToken = null;

                do
                {
                    var response = await client.GetParametersByPathAsync(
                        new GetParametersByPathRequest
                        {
                            Path = path,
                            Recursive = recursive,
                            MaxResults = 10,
                            NextToken = nextToken,
                        },
                        token);

                    foreach (var entry in response.Parameters ?? [])
                        parameters.Add(ToParameter(entry));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return parameters;
            },
            cancellationToken);

    public async Task<Result> CreateParameterAsync(
        ParameterSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSimpleSystemsManagementClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutParameterAsync(
                    new PutParameterRequest
                    {
                        Name = specification.Name,
                        Type = ParameterType.FindValue(specification.Type),
                        Value = specification.Value,
                        Description = specification.Description,
                        Overwrite = true,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteParameterAsync(string name, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSimpleSystemsManagementClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteParameterAsync(
                    new DeleteParameterRequest { Name = name },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<ParameterValue>> GetParameterValueAsync(string name, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSimpleSystemsManagementClient, ParameterValue>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetParameterAsync(
                    new GetParameterRequest { Name = name, WithDecryption = true },
                    token);

                var parameter = response.Parameter;
                return new ParameterValue(
                    parameter?.Name ?? string.Empty,
                    parameter?.Type?.Value ?? string.Empty,
                    parameter?.Version ?? 0,
                    parameter?.Value ?? string.Empty,
                    parameter?.ARN ?? string.Empty);
            },
            cancellationToken);

    public Task<Result<ParameterHistoryList>> GetParameterHistoryAsync(
        string name, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSimpleSystemsManagementClient, ParameterHistoryList>(
            ServiceKey,
            async (client, token) =>
            {
                var entries = new List<ParameterHistoryEntry>();
                string? nextToken = null;

                do
                {
                    var response = await client.GetParameterHistoryAsync(
                        new GetParameterHistoryRequest
                        {
                            Name = name,
                            WithDecryption = true,
                            MaxResults = 50,
                            NextToken = nextToken,
                        },
                        token);

                    foreach (var entry in response.Parameters ?? [])
                        entries.Add(ToParameterHistoryEntry(entry));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return new ParameterHistoryList(name, entries);
            },
            cancellationToken);

    public async Task<Result> PutParameterValueAsync(
        ParameterValueSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSimpleSystemsManagementClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutParameterAsync(
                    new PutParameterRequest
                    {
                        Name = specification.Name,
                        Value = specification.Value,
                        Overwrite = true,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static Parameter ToParameter(AwsParameter entry)
        => new(
            entry.Name ?? string.Empty,
            entry.Type?.Value ?? string.Empty,
            entry.Version ?? 0,
            entry.LastModifiedDate is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(entry.LastModifiedDate.Value, DateTimeKind.Utc)),
            entry.ARN ?? string.Empty);

    private static ParameterHistoryEntry ToParameterHistoryEntry(ParameterHistory entry)
        => new(
            entry.Type?.Value ?? string.Empty,
            entry.Version ?? 0,
            entry.Value ?? string.Empty,
            entry.LastModifiedDate is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(entry.LastModifiedDate.Value, DateTimeKind.Utc)),
            entry.LastModifiedUser ?? string.Empty);
}
