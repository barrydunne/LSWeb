using Foundation.Application.Queries.SearchResources;
using Foundation.Application.Search;
using Foundation.Domain.Search;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.SearchResources;

public class SearchResourcesQueryHandlerTests
{
    private readonly ISearchIndexProvider _provider = Substitute.For<ISearchIndexProvider>();

    private static SearchIndexState Snapshot(params SearchEntry[] entries)
        => new(entries, DateTimeOffset.UtcNow);

    [Fact]
    public async Task Handle_WhenQueryIsBlank_ReturnsNoMatchesWithoutReadingTheIndex()
    {
        // Arrange
        var sut = new SearchResourcesQueryHandler(_provider, NullLogger<SearchResourcesQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new SearchResourcesQuery("   "), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Matches.Should().BeEmpty();
        _provider.DidNotReceive().GetCurrent();
    }

    [Fact]
    public async Task Handle_WhenQueryMatchesDisplayName_ReturnsMatchingEntry()
    {
        // Arrange
        var entry = new SearchEntry("sqs", "id-1", "Orders Queue", "/services/sqs/id-1");
        _provider.GetCurrent().Returns(Snapshot(entry));
        var sut = new SearchResourcesQueryHandler(_provider, NullLogger<SearchResourcesQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new SearchResourcesQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.Value.Matches.Should().ContainSingle().Which.Should().Be(entry);
    }

    [Fact]
    public async Task Handle_WhenQueryMatchesResourceId_ReturnsMatchingEntry()
    {
        // Arrange
        var entry = new SearchEntry("sqs", "payments-id", "Some Queue", "/services/sqs/payments-id");
        _provider.GetCurrent().Returns(Snapshot(entry));
        var sut = new SearchResourcesQueryHandler(_provider, NullLogger<SearchResourcesQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new SearchResourcesQuery("PAYMENTS"), TestContext.Current.CancellationToken);

        // Assert
        result.Value.Matches.Should().ContainSingle().Which.Should().Be(entry);
    }

    [Fact]
    public async Task Handle_WhenQueryMatchesServiceKey_ReturnsMatchingEntry()
    {
        // Arrange
        var entry = new SearchEntry("dynamodb", "id-9", "A Table", "/services/dynamodb/id-9");
        _provider.GetCurrent().Returns(Snapshot(entry));
        var sut = new SearchResourcesQueryHandler(_provider, NullLogger<SearchResourcesQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new SearchResourcesQuery("dynamo"), TestContext.Current.CancellationToken);

        // Assert
        result.Value.Matches.Should().ContainSingle().Which.Should().Be(entry);
    }

    [Fact]
    public async Task Handle_WhenQueryMatchesNothing_ReturnsEmpty()
    {
        // Arrange
        var entry = new SearchEntry("sqs", "id-1", "Orders Queue", "/services/sqs/id-1");
        _provider.GetCurrent().Returns(Snapshot(entry));
        var sut = new SearchResourcesQueryHandler(_provider, NullLogger<SearchResourcesQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new SearchResourcesQuery("nothing-here"), TestContext.Current.CancellationToken);

        // Assert
        result.Value.Matches.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenQueryIsNull_ReturnsNoMatches()
    {
        // Arrange
        var sut = new SearchResourcesQueryHandler(_provider, NullLogger<SearchResourcesQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new SearchResourcesQuery(null!), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Matches.Should().BeEmpty();
    }
}
