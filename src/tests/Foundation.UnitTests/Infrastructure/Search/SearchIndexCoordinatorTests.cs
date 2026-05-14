using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class SearchIndexCoordinatorTests
{
    [Fact]
    public void IsBuilding_BeforeAnyBuild_IsFalse()
    {
        using var sut = new SearchIndexCoordinator();

        sut.IsBuilding.Should().BeFalse();
    }

    [Fact]
    public void BeginBuild_ThenEndBuild_TogglesIsBuilding()
    {
        using var sut = new SearchIndexCoordinator();

        sut.BeginBuild();
        sut.IsBuilding.Should().BeTrue();

        sut.EndBuild();
        sut.IsBuilding.Should().BeFalse();
    }

    [Fact]
    public async Task WaitForRefreshAsync_WhenRefreshRequested_ReturnsTrue()
    {
        // Arrange
        using var sut = new SearchIndexCoordinator();
        sut.RequestRefresh();

        // Act
        var signalled = await sut.WaitForRefreshAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        signalled.Should().BeTrue();
    }

    [Fact]
    public async Task WaitForRefreshAsync_WhenIntervalElapses_ReturnsFalse()
    {
        // Arrange
        using var sut = new SearchIndexCoordinator();

        // Act
        var signalled = await sut.WaitForRefreshAsync(TimeSpan.Zero, TestContext.Current.CancellationToken);

        // Assert
        signalled.Should().BeFalse();
    }

    [Fact]
    public async Task RequestRefresh_WhenCalledRepeatedly_CoalescesIntoASinglePendingRefresh()
    {
        // Arrange
        using var sut = new SearchIndexCoordinator();

        // Act
        sut.RequestRefresh();
        sut.RequestRefresh();
        sut.RequestRefresh();

        // Assert - exactly one pending refresh is consumed, the next wait times out.
        var first = await sut.WaitForRefreshAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        var second = await sut.WaitForRefreshAsync(TimeSpan.Zero, TestContext.Current.CancellationToken);

        first.Should().BeTrue();
        second.Should().BeFalse();
    }
}
