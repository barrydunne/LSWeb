using Foundation.Domain.Capabilities;
using Foundation.Domain.Catalogue;
using Foundation.Domain.Errors;
using Foundation.Infrastructure.Capabilities;

namespace Foundation.UnitTests.Infrastructure.Capabilities;

public class CapabilityDetectorTests
{
    private readonly CapabilityDetector _sut = new();

    [Fact]
    public void GetCapabilities_Initially_ReturnsAllCatalogueServicesUnknown()
    {
        var map = _sut.GetCapabilities();

        map.Entries.Select(entry => entry.Key)
            .Should()
            .BeEquivalentTo(ServiceCatalogue.Services.Select(service => service.Key));
        map.Entries.Should().OnlyContain(entry => entry.Status == CapabilityStatus.Unknown && entry.Detail == null);
    }

    [Fact]
    public void GetCapabilities_ReturnsEntriesOrderedByKey()
    {
        var keys = _sut.GetCapabilities().Entries.Select(entry => entry.Key).ToList();

        keys.Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [Fact]
    public void RecordSuccess_MarksServiceSupported()
    {
        _sut.RecordSuccess("s3");

        _sut.GetCapabilities().IsSupported("s3").Should().BeTrue();
    }

    [Fact]
    public void RecordError_WhenUnsupportedCategory_MarksServiceUnsupported()
    {
        var error = new ErrorModel("NotImplemented", "nope", ErrorCategory.Unsupported, ErrorClassification.Terminal);

        _sut.RecordError("lambda", error);

        var entry = _sut.GetCapabilities().Find("lambda");
        entry!.Status.Should().Be(CapabilityStatus.Unsupported);
        entry.Detail.Should().Be("Not supported by the current backend.");
    }

    [Fact]
    public void RecordError_WhenOtherCategory_MarksServiceSupported()
    {
        var error = new ErrorModel("Throttling", "busy", ErrorCategory.Throttling, ErrorClassification.Retryable);

        _sut.RecordError("sqs", error);

        _sut.GetCapabilities().Find("sqs")!.Status.Should().Be(CapabilityStatus.Supported);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RecordSuccess_WhenKeyInvalid_Throws(string? serviceKey)
    {
        var act = () => _sut.RecordSuccess(serviceKey!);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RecordError_WhenKeyInvalid_Throws(string? serviceKey)
    {
        var error = new ErrorModel("Code", "msg", ErrorCategory.Unknown, ErrorClassification.Terminal);

        var act = () => _sut.RecordError(serviceKey!, error);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordError_WhenErrorNull_Throws()
    {
        var act = () => _sut.RecordError("s3", null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
