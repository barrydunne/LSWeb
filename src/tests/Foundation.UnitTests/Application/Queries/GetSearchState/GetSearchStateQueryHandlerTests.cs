using Foundation.Application.Queries.GetSearchState;
using Foundation.Application.Search;
using Foundation.Domain.Search;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSearchState;

public class GetSearchStateQueryHandlerTests
{
    private readonly ISearchIndexProvider _provider = Substitute.For<ISearchIndexProvider>();
    private readonly ISearchIndexSignals _signals = Substitute.For<ISearchIndexSignals>();

    [Fact]
    public async Task Handle_WhenInvoked_ReturnsBuiltAtAndEntryCountFromSnapshot()
    {
        // Arrange
        var builtAt = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var entries = new List<SearchEntry>
        {
            new("sqs", "id-1", "Orders Queue", "/services/sqs/id-1"),
            new("dynamodb", "id-2", "A Table", "/services/dynamodb/id-2"),
        };
        _provider.GetCurrent().Returns(new SearchIndexState(entries, builtAt));
        _signals.IsBuilding.Returns(false);
        var sut = new GetSearchStateQueryHandler(_provider, _signals, NullLogger<GetSearchStateQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetSearchStateQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BuiltAt.Should().Be(builtAt);
        result.Value.EntryCount.Should().Be(2);
        result.Value.IsBuilding.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenRebuildInProgress_ReportsBuildingTrue()
    {
        // Arrange
        _provider.GetCurrent().Returns(SearchIndexState.Empty);
        _signals.IsBuilding.Returns(true);
        var sut = new GetSearchStateQueryHandler(_provider, _signals, NullLogger<GetSearchStateQueryHandler>.Instance);

        // Act
        var result = await sut.Handle(new GetSearchStateQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.Value.IsBuilding.Should().BeTrue();
        result.Value.EntryCount.Should().Be(0);
    }
}
