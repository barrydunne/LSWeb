using Foundation.Application.Capabilities;
using Foundation.Application.Queries.GetCatalogue;
using Foundation.Domain.Capabilities;
using Foundation.Domain.Catalogue;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetCatalogue;

public class GetCatalogueQueryHandlerTests
{
    private readonly ICapabilityProvider _capabilityProvider = Substitute.For<ICapabilityProvider>();

    private GetCatalogueQueryHandler CreateSut()
        => new(_capabilityProvider, NullLogger<GetCatalogueQueryHandler>.Instance);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        // Arrange
        _capabilityProvider.GetCapabilities().Returns(CapabilityMap.Empty);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetCatalogueQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsTheManagedServiceCatalogue()
    {
        // Arrange
        _capabilityProvider.GetCapabilities().Returns(CapabilityMap.Empty);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetCatalogueQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.Value.Services.Should().BeEquivalentTo(ServiceCatalogue.Services);
    }

    [Fact]
    public async Task Handle_IncludesTheCapabilitySnapshot()
    {
        // Arrange
        var capabilities = new CapabilityMap(
        [
            new CapabilityEntry("s3", CapabilityStatus.Unsupported, "Not supported by the current backend."),
        ]);
        _capabilityProvider.GetCapabilities().Returns(capabilities);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetCatalogueQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.Value.Capabilities.Should().BeSameAs(capabilities);
    }
}
