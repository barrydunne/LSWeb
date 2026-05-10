using Foundation.Domain.Catalogue;
using Foundation.Domain.Health;
using Foundation.Infrastructure.Health;

namespace Foundation.UnitTests.Infrastructure.Health;

public class HealthStatusStoreTests
{
    [Fact]
    public void GetCurrent_WhenNotUpdated_ReturnsAllServicesUnknown()
    {
        // Arrange
        var sut = new HealthStatusStore();

        // Act
        var snapshot = sut.GetCurrent();

        // Assert
        snapshot.Services.Should().HaveCount(ServiceCatalogue.Services.Count);
        snapshot.Services.Should().OnlyContain(_ => _.Availability == ServiceAvailability.Unknown);
    }

    [Fact]
    public void GetCurrent_AfterUpdate_ReturnsUpdatedSnapshot()
    {
        // Arrange
        var sut = new HealthStatusStore();
        var updated = new HealthStatus([new ServiceHealth("s3", ServiceAvailability.Available)]);

        // Act
        sut.Update(updated);

        // Assert
        sut.GetCurrent().Should().BeSameAs(updated);
    }
}
