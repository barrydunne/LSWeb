using Foundation.Application.Preferences;
using Foundation.Application.Search;
using Foundation.Domain.Preferences;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.IntegrationTests.Preferences;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class RecentlyViewedPrunerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public RecentlyViewedPrunerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task PruneAsync_AfterTheIndexIsBuilt_RemovesReferencesToResourcesThatDoNotExist()
    {
        // Arrange: starting the host begins the background indexer.
        _ = _fixture.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;
        var userDataStore = _fixture.Services.GetRequiredService<IUserDataStore>();
        var searchIndexProvider = _fixture.Services.GetRequiredService<ISearchIndexProvider>();
        var pruner = _fixture.Services.GetRequiredService<IRecentlyViewedPruner>();

        var missingReference = $"sqs://lsw-missing-{Guid.NewGuid():N}";
        await userDataStore.SavePreferencesAsync(
            new UserPreferences([], [missingReference]),
            cancellationToken);

        await WaitForFirstBuildAsync(searchIndexProvider, cancellationToken);

        // Act
        var result = await pruner.PruneAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var preferences = await userDataStore.GetPreferencesAsync(cancellationToken);
        preferences.IsSuccess.Should().BeTrue();
        preferences.Value.RecentlyViewed.Should().NotContain(missingReference);
    }

    private static async Task WaitForFirstBuildAsync(
        ISearchIndexProvider provider,
        CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (provider.GetCurrent().BuiltAt > DateTimeOffset.MinValue)
                return;

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }
    }
}
