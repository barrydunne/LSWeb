using Foundation.Application.Queries.GetSeedTemplates;
using Foundation.Application.Seed;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSeedTemplates;

public class GetSeedTemplatesQueryHandlerTests
{
    private readonly SeedTemplateCatalogue _catalogue = new();

    [Fact]
    public async Task Handle_ReturnsTemplatesFromCatalogue()
    {
        // Arrange
        var sut = new GetSeedTemplatesQueryHandler(_catalogue, NullLogger<GetSeedTemplatesQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetSeedTemplatesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Templates.Should().BeEquivalentTo(_catalogue.GetTemplates());
    }
}
