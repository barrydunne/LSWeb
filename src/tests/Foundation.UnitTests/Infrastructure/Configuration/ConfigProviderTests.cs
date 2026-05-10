using Foundation.Domain.Configuration;
using Foundation.Infrastructure.Configuration;

namespace Foundation.UnitTests.Infrastructure.Configuration;

public class ConfigProviderTests
{
    [Fact]
    public void GetSnapshot_WhenNoSettingsProvided_AppliesDefaultsMarkedAsDefaultSource()
    {
        // Arrange
        var sut = new ConfigProvider(new AwsSettings());

        // Act
        var snapshot = sut.GetSnapshot();

        // Assert
        snapshot.AccessKey.Value.Should().Be("test");
        snapshot.AccessKey.Source.Should().Be(ConfigSource.Default);
        snapshot.SecretKey.Value.Should().Be("test");
        snapshot.ServiceUrl.Value.Should().Be("http://host.docker.internal:4566");
        snapshot.ServiceUrl.Source.Should().Be(ConfigSource.Default);
        snapshot.Region.Value.Should().Be("eu-west-1");
    }

    [Fact]
    public void GetSnapshot_WhenSettingsProvided_UsesValuesMarkedAsEnvironmentSource()
    {
        // Arrange
        var sut = new ConfigProvider(new AwsSettings
        {
            AccessKey = "AKIA123",
            SecretKey = "secret",
            ServiceUrl = "http://localhost:4566",
            Region = "us-east-1",
        });

        // Act
        var snapshot = sut.GetSnapshot();

        // Assert
        snapshot.AccessKey.Value.Should().Be("AKIA123");
        snapshot.AccessKey.Source.Should().Be(ConfigSource.EnvironmentVariable);
        snapshot.ServiceUrl.Value.Should().Be("http://localhost:4566");
        snapshot.ServiceUrl.Source.Should().Be(ConfigSource.EnvironmentVariable);
        snapshot.Region.Value.Should().Be("us-east-1");
        snapshot.Region.Source.Should().Be(ConfigSource.EnvironmentVariable);
    }

    [Fact]
    public void GetSnapshot_WhenSettingIsWhitespace_FallsBackToDefault()
    {
        // Arrange
        var sut = new ConfigProvider(new AwsSettings { Region = "   " });

        // Act
        var snapshot = sut.GetSnapshot();

        // Assert
        snapshot.Region.Value.Should().Be("eu-west-1");
        snapshot.Region.Source.Should().Be(ConfigSource.Default);
    }

    [Fact]
    public void GetSnapshot_MasksSensitiveValuesButNotPublicOnes()
    {
        // Arrange
        var sut = new ConfigProvider(new AwsSettings { AccessKey = "AKIA123", ServiceUrl = "http://localhost:4566" });

        // Act
        var snapshot = sut.GetSnapshot();

        // Assert
        snapshot.AccessKey.Display.Should().Be("********");
        snapshot.SecretKey.Display.Should().Be("********");
        snapshot.ServiceUrl.Display.Should().Be("http://localhost:4566");
        snapshot.Region.Display.Should().Be("eu-west-1");
    }
}
