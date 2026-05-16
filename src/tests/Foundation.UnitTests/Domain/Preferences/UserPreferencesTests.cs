using Foundation.Domain.Preferences;

namespace Foundation.UnitTests.Domain.Preferences;

public class UserPreferencesTests
{
    [Fact]
    public void Empty_WhenRead_HasNoFavouritesOrRecentlyViewed()
    {
        // Act
        var preferences = UserPreferences.Empty;

        // Assert
        preferences.Favourites.Should().BeEmpty();
        preferences.RecentlyViewed.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WhenGivenCollections_ExposesThemUnchanged()
    {
        // Arrange
        var favourites = new[] { "s3://bucket/object" };
        var recentlyViewed = new[] { "sqs://queue", "sns://topic" };

        // Act
        var preferences = new UserPreferences(favourites, recentlyViewed);

        // Assert
        preferences.Favourites.Should().BeSameAs(favourites);
        preferences.RecentlyViewed.Should().BeSameAs(recentlyViewed);
    }

    [Fact]
    public void WithRecentlyViewed_WhenReferenceIsNew_AddsItToTheFront()
    {
        // Arrange
        var preferences = new UserPreferences([], ["sqs://queue"]);

        // Act
        var updated = preferences.WithRecentlyViewed("sns://topic", maxItems: 10);

        // Assert
        updated.RecentlyViewed.Should().ContainInOrder("sns://topic", "sqs://queue");
    }

    [Fact]
    public void WithRecentlyViewed_WhenReferenceAlreadyPresent_MovesItToTheFrontWithoutDuplicating()
    {
        // Arrange
        var preferences = new UserPreferences([], ["sqs://queue", "sns://topic", "s3://bucket"]);

        // Act
        var updated = preferences.WithRecentlyViewed("sns://topic", maxItems: 10);

        // Assert
        updated.RecentlyViewed.Should().ContainInOrder("sns://topic", "sqs://queue", "s3://bucket");
        updated.RecentlyViewed.Should().HaveCount(3);
    }

    [Fact]
    public void WithRecentlyViewed_WhenListExceedsMax_TrimsToMaxItems()
    {
        // Arrange
        var preferences = new UserPreferences([], ["a", "b", "c"]);

        // Act
        var updated = preferences.WithRecentlyViewed("d", maxItems: 3);

        // Assert
        updated.RecentlyViewed.Should().ContainInOrder("d", "a", "b");
        updated.RecentlyViewed.Should().HaveCount(3);
    }

    [Fact]
    public void WithFavourite_WhenReferenceIsNew_AppendsItToTheEnd()
    {
        // Arrange
        var preferences = new UserPreferences(["s3://bucket"], []);

        // Act
        var updated = preferences.WithFavourite("sqs://queue");

        // Assert
        updated.Favourites.Should().ContainInOrder("s3://bucket", "sqs://queue");
    }

    [Fact]
    public void WithFavourite_WhenReferenceAlreadyPinned_ReturnsTheSameInstance()
    {
        // Arrange
        var preferences = new UserPreferences(["s3://bucket"], []);

        // Act
        var updated = preferences.WithFavourite("s3://bucket");

        // Assert
        updated.Should().BeSameAs(preferences);
    }

    [Fact]
    public void WithoutFavourite_WhenReferencePinned_RemovesIt()
    {
        // Arrange
        var preferences = new UserPreferences(["s3://bucket", "sqs://queue"], []);

        // Act
        var updated = preferences.WithoutFavourite("s3://bucket");

        // Assert
        updated.Favourites.Should().ContainSingle().Which.Should().Be("sqs://queue");
    }

    [Fact]
    public void WithoutFavourite_WhenReferenceNotPinned_LeavesFavouritesUnchanged()
    {
        // Arrange
        var preferences = new UserPreferences(["s3://bucket"], []);

        // Act
        var updated = preferences.WithoutFavourite("sqs://queue");

        // Assert
        updated.Favourites.Should().ContainInOrder("s3://bucket");
    }
}
