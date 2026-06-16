using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Navigation;
using Foundation.Application.Preferences;
using Foundation.Application.Search;
using Foundation.Domain.Navigation;
using Foundation.Domain.Preferences;
using Foundation.Domain.Search;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Preferences;

public class RecentlyViewedPrunerTests
{
    private readonly IUserDataStore _userDataStore = Substitute.For<IUserDataStore>();
    private readonly ISearchIndexProvider _searchIndexProvider = Substitute.For<ISearchIndexProvider>();
    private readonly IReferenceResolver _referenceResolver = Substitute.For<IReferenceResolver>();
    private readonly RecentlyViewedPruner _sut;

    public RecentlyViewedPrunerTests()
        => _sut = new RecentlyViewedPruner(
            _userDataStore,
            _searchIndexProvider,
            _referenceResolver,
            NullLogger<RecentlyViewedPruner>.Instance);

    [Fact]
    public async Task PruneAsync_WhenIndexHasNotBeenBuilt_ReturnsSuccessWithoutReadingPreferences()
    {
        // Arrange
        _searchIndexProvider.GetCurrent().Returns(SearchIndexState.Empty);

        // Act
        var result = await _sut.PruneAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userDataStore.DidNotReceive().GetPreferencesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PruneAsync_WhenPreferencesCannotBeRead_PropagatesTheError()
    {
        // Arrange
        BuildIndex(new SearchEntry("sqs", "queue", "queue", "/services/sqs/queue"));
        _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(new InvalidOperationException("boom")));

        // Act
        var result = await _sut.PruneAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _userDataStore.DidNotReceive().SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PruneAsync_WhenRecentlyViewedIsEmpty_ReturnsSuccessWithoutSaving()
    {
        // Arrange
        BuildIndex(new SearchEntry("sqs", "queue", "queue", "/services/sqs/queue"));
        StorePreferences(UserPreferences.Empty);

        // Act
        var result = await _sut.PruneAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userDataStore.DidNotReceive().SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PruneAsync_WhenEveryReferenceStillExists_ReturnsSuccessWithoutSaving()
    {
        // Arrange
        BuildIndex(new SearchEntry("sqs", "queue", "queue", "/services/sqs/queue"));
        StorePreferences(new UserPreferences([], ["sqs://queue"]));
        ResolvesTo("sqs://queue", new ResourceReference("sqs", "queue", "/services/sqs/queue"));

        // Act
        var result = await _sut.PruneAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userDataStore.DidNotReceive().SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PruneAsync_WhenAReferenceNoLongerExists_RemovesItAndSavesTheTrimmedList()
    {
        // Arrange
        BuildIndex(new SearchEntry("sqs", "queue", "queue", "/services/sqs/queue"));
        StorePreferences(new UserPreferences(["s3://bucket"], ["sqs://queue", "sqs://gone"]));
        ResolvesTo("sqs://queue", new ResourceReference("sqs", "queue", "/services/sqs/queue"));
        ResolvesTo("sqs://gone", new ResourceReference("sqs", "gone", "/services/sqs/gone"));
        _userDataStore
            .SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _sut.PruneAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userDataStore.Received(1).SavePreferencesAsync(
            Arg.Is<UserPreferences>(_ =>
                _.RecentlyViewed.Count == 1
                && _.RecentlyViewed[0] == "sqs://queue"
                && _.Favourites.Count == 1
                && _.Favourites[0] == "s3://bucket"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PruneAsync_WhenAReferenceCannotBeResolved_KeepsItWhilePruningResolvableMisses()
    {
        // Arrange
        BuildIndex(new SearchEntry("sqs", "queue", "queue", "/services/sqs/queue"));
        StorePreferences(new UserPreferences([], ["cloudformation://stack", "sqs://gone"]));
        _referenceResolver
            .Resolve("cloudformation://stack")
            .Returns((Result<ResourceReference>)new Error("Unsupported service 'cloudformation'."));
        ResolvesTo("sqs://gone", new ResourceReference("sqs", "gone", "/services/sqs/gone"));
        _userDataStore
            .SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _sut.PruneAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userDataStore.Received(1).SavePreferencesAsync(
            Arg.Is<UserPreferences>(_ =>
                _.RecentlyViewed.Count == 1
                && _.RecentlyViewed[0] == "cloudformation://stack"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PruneAsync_WhenSavingTheTrimmedListFails_PropagatesTheError()
    {
        // Arrange
        BuildIndex(new SearchEntry("sqs", "queue", "queue", "/services/sqs/queue"));
        StorePreferences(new UserPreferences([], ["sqs://gone"]));
        ResolvesTo("sqs://gone", new ResourceReference("sqs", "gone", "/services/sqs/gone"));
        _userDataStore
            .SavePreferencesAsync(Arg.Any<UserPreferences>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new InvalidOperationException("save failed")));

        // Act
        var result = await _sut.PruneAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    private void BuildIndex(params SearchEntry[] entries)
        => _searchIndexProvider.GetCurrent().Returns(new SearchIndexState(entries, DateTimeOffset.UtcNow));

    private void StorePreferences(UserPreferences preferences)
        => _userDataStore
            .GetPreferencesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPreferences>>(preferences));

    private void ResolvesTo(string reference, ResourceReference resolved)
        => _referenceResolver.Resolve(reference).Returns(resolved);
}
