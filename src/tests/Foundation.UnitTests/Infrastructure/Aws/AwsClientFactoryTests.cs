using Amazon.SecurityToken;
using Foundation.Infrastructure.Aws;
using Foundation.Infrastructure.Configuration;

namespace Foundation.UnitTests.Infrastructure.Aws;

public class AwsClientFactoryTests
{
    [Fact]
    public void CreateClient_WhenCalledTwiceForSameType_ReturnsCachedInstance()
    {
        // Arrange
        var provider = new ConfigProvider(new AwsSettings { ServiceUrl = "http://localhost:4566", Region = "eu-west-1" });
        using var sut = new AwsClientFactory(provider);

        // Act
        var first = sut.CreateClient<AmazonSecurityTokenServiceClient>();
        var second = sut.CreateClient<AmazonSecurityTokenServiceClient>();

        // Assert
        first.Should().NotBeNull();
        second.Should().BeSameAs(first);
    }

    [Fact]
    public void CreateClient_AppliesResolvedServiceUrlAndRegionToTheClientConfig()
    {
        // Arrange
        var provider = new ConfigProvider(new AwsSettings { ServiceUrl = "http://localhost:4566", Region = "eu-west-1" });
        using var sut = new AwsClientFactory(provider);

        // Act
        var client = sut.CreateClient<AmazonSecurityTokenServiceClient>();

        // Assert
        client.Config.ServiceURL.Should().StartWith("http://localhost:4566");
        client.Config.AuthenticationRegion.Should().Be("eu-west-1");
    }

    [Fact]
    public void Dispose_WhenClientsCreated_DoesNotThrow()
    {
        // Arrange
        var provider = new ConfigProvider(new AwsSettings());
        var sut = new AwsClientFactory(provider);
        _ = sut.CreateClient<AmazonSecurityTokenServiceClient>();

        // Act
        var dispose = sut.Dispose;

        // Assert
        dispose.Should().NotThrow();
    }
}
