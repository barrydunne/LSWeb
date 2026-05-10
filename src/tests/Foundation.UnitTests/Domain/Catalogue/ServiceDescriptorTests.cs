using Foundation.Domain.Catalogue;

namespace Foundation.UnitTests.Domain.Catalogue;

public class ServiceDescriptorTests
{
    [Fact]
    public void Constructor_ExposesAllProperties()
    {
        var descriptor = new ServiceDescriptor("s3", "S3", ServiceCategory.Storage, "archive", "/services/s3");

        descriptor.Key.Should().Be("s3");
        descriptor.DisplayName.Should().Be("S3");
        descriptor.Category.Should().Be(ServiceCategory.Storage);
        descriptor.IconHint.Should().Be("archive");
        descriptor.Route.Should().Be("/services/s3");
    }

    [Fact]
    public void Equality_TreatsDescriptorsWithSameValuesAsEqual()
    {
        var first = new ServiceDescriptor("s3", "S3", ServiceCategory.Storage, "archive", "/services/s3");
        var second = new ServiceDescriptor("s3", "S3", ServiceCategory.Storage, "archive", "/services/s3");

        first.Should().Be(second);
    }

    [Fact]
    public void Equality_TreatsDescriptorsWithDifferentValuesAsNotEqual()
    {
        var first = new ServiceDescriptor("s3", "S3", ServiceCategory.Storage, "archive", "/services/s3");
        var second = new ServiceDescriptor("sqs", "SQS", ServiceCategory.Messaging, "inbox", "/services/sqs");

        first.Should().NotBe(second);
    }
}
