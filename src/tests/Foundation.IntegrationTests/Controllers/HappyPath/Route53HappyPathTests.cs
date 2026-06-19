using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for Route 53: create a hosted zone, add a record set, confirm both are
/// listed, then delete the record. Route 53 exposes no hosted-zone delete endpoint, so the zone is
/// left for the ephemeral container teardown to discard.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class Route53HappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public Route53HappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateZoneUpsertRecordListDeleteRecord_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var unique = Guid.NewGuid().ToString("N");
        var zoneName = $"itest-r53-{unique}.example.com.";
        var recordName = $"www.{zoneName}";
        var recordValues = new[] { "192.0.2.1" };

        var createZoneResponse = await client.PostAsJsonAsync(
            "/api/services/route53/hostedzones",
            new HostedZoneCreateRequest(zoneName, "Integration test zone"),
            cancellationToken);
        createZoneResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var zones = await client.GetFromJsonAsync<HostedZoneListResponse>(
            "/api/services/route53/hostedzones", cancellationToken);
        zones.Should().NotBeNull();
        var createdZone = zones!.HostedZones.Should().ContainSingle(zone => zone.Name.Contains(unique)).Subject;
        var zoneId = createdZone.Id;

        var upsertResponse = await client.PutAsJsonAsync(
            $"/api/services/route53/records?zoneId={Uri.EscapeDataString(zoneId)}",
            new Route53RecordRequest(recordName, "A", 300, recordValues),
            cancellationToken);
        upsertResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var records = await client.GetFromJsonAsync<Route53RecordListResponse>(
            $"/api/services/route53/records?zoneId={Uri.EscapeDataString(zoneId)}", cancellationToken);
        records.Should().NotBeNull();
        records!.Records.Should().Contain(record => record.Name == recordName && record.Type == "A");

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/services/route53/records?zoneId={Uri.EscapeDataString(zoneId)}")
        {
            Content = JsonContent.Create(new Route53RecordRequest(recordName, "A", 300, recordValues)),
        };
        var deleteResponse = await client.SendAsync(deleteRequest, cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
