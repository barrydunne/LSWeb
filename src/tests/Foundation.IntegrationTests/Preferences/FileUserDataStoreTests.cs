using Foundation.Domain.Preferences;
using Foundation.Infrastructure.Preferences;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.IntegrationTests.Preferences;

public sealed class FileUserDataStoreTests : IDisposable
{
    private readonly string _directory;
    private readonly FileUserDataStore _sut;

    public FileUserDataStoreTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"lsw-user-data-{Guid.NewGuid():N}");
        var settings = new UserDataSettings { DataDirectory = _directory };
        _sut = new FileUserDataStore(settings, NullLogger<FileUserDataStore>.Instance);
    }

    [Fact]
    public async Task GetPreferencesAsync_WhenNoFileExists_ReturnsEmptyPreferences()
    {
        // Act
        var result = await _sut.GetPreferencesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Favourites.Should().BeEmpty();
        result.Value.RecentlyViewed.Should().BeEmpty();
    }

    [Fact]
    public async Task SavePreferencesAsync_WhenInvoked_WritesAFileToTheConfiguredDirectory()
    {
        // Arrange
        var preferences = new UserPreferences(["s3://bucket/object"], ["sqs://queue"]);

        // Act
        var result = await _sut.SavePreferencesAsync(preferences, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(Path.Combine(_directory, "preferences.json")).Should().BeTrue();
    }

    [Fact]
    public async Task GetPreferencesAsync_AfterSave_ReturnsTheRoundTrippedPreferences()
    {
        // Arrange
        var preferences = new UserPreferences(["s3://bucket/object"], ["sqs://queue", "sns://topic"]);
        await _sut.SavePreferencesAsync(preferences, TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPreferencesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Favourites.Should().Equal(preferences.Favourites);
        result.Value.RecentlyViewed.Should().Equal(preferences.RecentlyViewed);
    }

    [Fact]
    public async Task GetPreferencesAsync_WhenTheFileIsCorrupt_ReturnsAFailure()
    {
        // Arrange
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(
            Path.Combine(_directory, "preferences.json"),
            "{ not valid json",
            TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPreferencesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
