using System.Diagnostics.CodeAnalysis;
using Amazon.Route53;
using Amazon.Route53.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Route53;
using Foundation.Infrastructure.Aws;
using DomainHostedZone = Foundation.Domain.Route53.HostedZone;
using DomainRecord = Foundation.Domain.Route53.Route53Record;
using SdkHostedZone = Amazon.Route53.Model.HostedZone;

namespace Foundation.Infrastructure.Route53;

/// <summary>
/// Reads Route 53 through the resilient AWS gateway so the same code works against LocalStack or
/// real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and
/// converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class Route53ClientAdapter : IRoute53Client
{
    private const string ServiceKey = "route53";

    private readonly IAwsGateway _gateway;

    public Route53ClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<DomainHostedZone>>> ListHostedZonesAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonRoute53Client, IReadOnlyList<DomainHostedZone>>(
            ServiceKey,
            async (client, token) =>
            {
                var hostedZones = new List<DomainHostedZone>();
                string? marker = null;

                do
                {
                    var response = await client.ListHostedZonesAsync(
                        new ListHostedZonesRequest { Marker = marker },
                        token);

                    foreach (var zone in response.HostedZones ?? [])
                        hostedZones.Add(ToHostedZone(zone));

                    marker = response.IsTruncated == true ? response.NextMarker : null;
                }
                while (!string.IsNullOrEmpty(marker));

                return hostedZones;
            },
            cancellationToken);

    public Task<Result<DomainHostedZone>> CreateHostedZoneAsync(
        string name, string? comment, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonRoute53Client, DomainHostedZone>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateHostedZoneRequest
                {
                    Name = name,
                    CallerReference = Guid.NewGuid().ToString(),
                };

                if (!string.IsNullOrEmpty(comment))
                    request.HostedZoneConfig = new HostedZoneConfig { Comment = comment };

                var response = await client.CreateHostedZoneAsync(request, token);
                return ToHostedZone(response.HostedZone);
            },
            cancellationToken);

    private static DomainHostedZone ToHostedZone(SdkHostedZone zone)
        => new(
            zone.Id ?? string.Empty,
            zone.Name ?? string.Empty,
            zone.ResourceRecordSetCount ?? 0,
            zone.Config?.PrivateZone ?? false);

    public Task<Result<IReadOnlyList<DomainRecord>>> ListRecordsAsync(
        string hostedZoneId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonRoute53Client, IReadOnlyList<DomainRecord>>(
            ServiceKey,
            async (client, token) =>
            {
                var records = new List<DomainRecord>();
                string? recordName = null;
                string? recordType = null;

                do
                {
                    var response = await client.ListResourceRecordSetsAsync(
                        new ListResourceRecordSetsRequest
                        {
                            HostedZoneId = hostedZoneId,
                            StartRecordName = recordName,
                            StartRecordType = recordType is null ? null : RRType.FindValue(recordType),
                        },
                        token);

                    foreach (var recordSet in response.ResourceRecordSets ?? [])
                        records.Add(ToRecord(recordSet));

                    if (response.IsTruncated == true)
                    {
                        recordName = response.NextRecordName;
                        recordType = response.NextRecordType?.Value;
                    }
                    else
                    {
                        recordName = null;
                        recordType = null;
                    }
                }
                while (!string.IsNullOrEmpty(recordName));

                return records;
            },
            cancellationToken);

    public async Task<Result> UpsertRecordAsync(
        string hostedZoneId, DomainRecord record, CancellationToken cancellationToken)
        => await ChangeRecordAsync(hostedZoneId, record, ChangeAction.UPSERT, cancellationToken);

    public async Task<Result> DeleteRecordAsync(
        string hostedZoneId, DomainRecord record, CancellationToken cancellationToken)
        => await ChangeRecordAsync(hostedZoneId, record, ChangeAction.DELETE, cancellationToken);

    private async Task<Result> ChangeRecordAsync(
        string hostedZoneId, DomainRecord record, ChangeAction action, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonRoute53Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.ChangeResourceRecordSetsAsync(
                    new ChangeResourceRecordSetsRequest
                    {
                        HostedZoneId = hostedZoneId,
                        ChangeBatch = new ChangeBatch
                        {
                            Changes =
                            [
                                new Change
                                {
                                    Action = action,
                                    ResourceRecordSet = new ResourceRecordSet
                                    {
                                        Name = record.Name,
                                        Type = RRType.FindValue(record.Type),
                                        TTL = record.Ttl,
                                        ResourceRecords = record.Values
                                            .Select(value => new ResourceRecord { Value = value })
                                            .ToList(),
                                    },
                                },
                            ],
                        },
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static DomainRecord ToRecord(ResourceRecordSet recordSet)
        => new(
            recordSet.Name ?? string.Empty,
            recordSet.Type?.Value ?? string.Empty,
            recordSet.TTL ?? 0,
            (recordSet.ResourceRecords ?? [])
                .Select(record => record.Value ?? string.Empty)
                .ToList());
}
