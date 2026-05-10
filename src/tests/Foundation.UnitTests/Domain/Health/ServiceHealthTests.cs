using Foundation.Domain.Health;

namespace Foundation.UnitTests.Domain.Health;

public class ServiceHealthTests
{
    [Fact]
    public void Constructor_ExposesAllProperties()
    {
        var health = new ServiceHealth("s3", ServiceAvailability.Available);

        health.Key.Should().Be("s3");
        health.Availability.Should().Be(ServiceAvailability.Available);
    }

    [Fact]
    public void Equality_TreatsValuesWithSameContentAsEqual()
    {
        var first = new ServiceHealth("s3", ServiceAvailability.Available);
        var second = new ServiceHealth("s3", ServiceAvailability.Available);

        first.Should().Be(second);
    }

    [Fact]
    public void Equality_TreatsValuesWithDifferentContentAsNotEqual()
    {
        var first = new ServiceHealth("s3", ServiceAvailability.Available);
        var second = new ServiceHealth("sqs", ServiceAvailability.Unavailable);

        first.Should().NotBe(second);
    }
}
