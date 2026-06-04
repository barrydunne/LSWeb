using System.Diagnostics.CodeAnalysis;
using Amazon.Route53;
using Amazon.Route53.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Route53;
using Foundation.Infrastructure.Aws;
using DomainHostedZone = Foundation.Domain.Route53.HostedZone;
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

    private static DomainHostedZone ToHostedZone(SdkHostedZone zone)
        => new(
            zone.Id ?? string.Empty,
            zone.Name ?? string.Empty,
            zone.ResourceRecordSetCount ?? 0,
            zone.Config?.PrivateZone ?? false);
}
