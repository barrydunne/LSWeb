using Foundation.Domain.Health;

namespace Foundation.UnitTests.Domain.Health;

public class HealthStatusTests
{
    [Fact]
    public void Constructor_ExposesServices()
    {
        var services = new[]
        {
            new ServiceHealth("s3", ServiceAvailability.Available),
            new ServiceHealth("sqs", ServiceAvailability.Unavailable),
        };

        var status = new HealthStatus(services);

        status.Services.Should().BeEquivalentTo(services);
    }

    [Fact]
    public void Equality_TreatsStatusesWithSameReferenceListAsEqual()
    {
        var services = new[] { new ServiceHealth("s3", ServiceAvailability.Available) };
        var first = new HealthStatus(services);
        var second = new HealthStatus(services);

        first.Should().Be(second);
    }
}
