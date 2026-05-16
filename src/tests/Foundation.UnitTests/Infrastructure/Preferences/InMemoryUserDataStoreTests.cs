using Foundation.Domain.Preferences;
using Foundation.Infrastructure.Preferences;

namespace Foundation.UnitTests.Infrastructure.Preferences;

public class InMemoryUserDataStoreTests
{
    [Fact]
    public async Task GetPreferencesAsync_WhenNothingSaved_ReturnsEmptyPreferences()
    {
        // Arrange
        var sut = new InMemoryUserDataStore();

        // Act
        var result = await sut.GetPreferencesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(UserPreferences.Empty);
    }

    [Fact]
    public async Task SavePreferencesAsync_WhenInvoked_ReportsSuccess()
    {
        // Arrange
        var sut = new InMemoryUserDataStore();
        var preferences = new UserPreferences(["s3://bucket"], ["sqs://queue"]);

        // Act
        var result = await sut.SavePreferencesAsync(preferences, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetPreferencesAsync_AfterSave_ReturnsTheSavedPreferences()
    {
        // Arrange
        var sut = new InMemoryUserDataStore();
        var preferences = new UserPreferences(["s3://bucket"], ["sqs://queue"]);
        await sut.SavePreferencesAsync(preferences, TestContext.Current.CancellationToken);

        // Act
        var result = await sut.GetPreferencesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(preferences);
    }

    [Fact]
    public async Task SavePreferencesAsync_WhenCalledTwice_ReplacesThePreviousPreferences()
    {
        // Arrange
        var sut = new InMemoryUserDataStore();
        var first = new UserPreferences(["s3://first"], []);
        var second = new UserPreferences(["s3://second"], ["sns://topic"]);
        await sut.SavePreferencesAsync(first, TestContext.Current.CancellationToken);

        // Act
        await sut.SavePreferencesAsync(second, TestContext.Current.CancellationToken);
        var result = await sut.GetPreferencesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Value.Should().BeSameAs(second);
    }
}
