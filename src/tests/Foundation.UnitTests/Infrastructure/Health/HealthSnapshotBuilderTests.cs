using Foundation.Domain.Catalogue;
using Foundation.Domain.Health;
using Foundation.Infrastructure.Health;

namespace Foundation.UnitTests.Infrastructure.Health;

public class HealthSnapshotBuilderTests
{
    [Fact]
    public void Build_WhenProbeFailed_MarksAllServicesAvailable()
    {
        // Arrange
        var probe = new BackendHealthResult(false, new HashSet<string>());

        // Act
        var snapshot = HealthSnapshotBuilder.Build(probe);

        // Assert
        snapshot.Services.Should().HaveCount(ServiceCatalogue.Services.Count);
        snapshot.Services.Should().OnlyContain(_ => _.Availability == ServiceAvailability.Available);
    }

    [Fact]
    public void Build_WhenProbeSucceeded_MarksOnlyReportedServicesAvailable()
    {
        // Arrange
        var available = new HashSet<string> { "s3", "lambda" };
        var probe = new BackendHealthResult(true, available);

        // Act
        var snapshot = HealthSnapshotBuilder.Build(probe);

        // Assert
        snapshot.Services.Should().HaveCount(ServiceCatalogue.Services.Count);
        snapshot.Services
            .Where(_ => available.Contains(_.Key))
            .Should().OnlyContain(_ => _.Availability == ServiceAvailability.Available);
        snapshot.Services
            .Where(_ => !available.Contains(_.Key))
            .Should().OnlyContain(_ => _.Availability == ServiceAvailability.Unavailable);
    }

    [Fact]
    public void Unknown_ReturnsEveryCatalogueServiceAsUnknown()
    {
        // Act
        var snapshot = HealthSnapshotBuilder.Unknown();

        // Assert
        snapshot.Services.Should().HaveCount(ServiceCatalogue.Services.Count);
        snapshot.Services.Should().OnlyContain(_ => _.Availability == ServiceAvailability.Unknown);
    }
}
