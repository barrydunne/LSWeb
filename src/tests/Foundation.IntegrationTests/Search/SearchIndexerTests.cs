using Foundation.Application.Search;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.IntegrationTests.Search;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SearchIndexerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SearchIndexerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task SearchIndexer_OnStartup_PublishesASnapshotIntoTheStore()
    {
        // Arrange: creating a client starts the host and its hosted services.
        _ = _fixture.CreateClient();
        var provider = _fixture.Services.GetRequiredService<ISearchIndexProvider>();

        // Act: the indexer rebuilds immediately on startup; wait for the first swap.
        var snapshot = await WaitForFirstBuildAsync(provider, TestContext.Current.CancellationToken);

        // Assert
        snapshot.Should().NotBeNull();
        snapshot!.BuiltAt.Should().BeAfter(DateTimeOffset.MinValue);
        snapshot.Entries.Should().NotBeNull();
    }

    private static async Task<Domain.Search.SearchIndexState?> WaitForFirstBuildAsync(
        ISearchIndexProvider provider,
        CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var current = provider.GetCurrent();
            if (current.BuiltAt > DateTimeOffset.MinValue)
                return current;

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }

        return null;
    }
}
