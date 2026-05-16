using Foundation.Application.Configuration;
using Foundation.Application.Queries.GenerateCliSnippet;
using Foundation.Domain.Configuration;
using Foundation.Domain.Snippets;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GenerateCliSnippet;

public class GenerateCliSnippetQueryHandlerTests
{
    private readonly IConfigProvider _configProvider = Substitute.For<IConfigProvider>();

    public GenerateCliSnippetQueryHandlerTests()
    {
        _configProvider.GetSnapshot().Returns(new ConfigSnapshot(
            new ConfigValue("AccessKey", "live-access", ConfigSource.EnvironmentVariable, IsSensitive: true),
            new ConfigValue("SecretKey", "live-secret", ConfigSource.EnvironmentVariable, IsSensitive: true),
            new ConfigValue("ServiceUrl", "http://localhost:4566", ConfigSource.Default, IsSensitive: false),
            new ConfigValue("Region", "eu-west-1", ConfigSource.Default, IsSensitive: false)));
    }

    private GenerateCliSnippetQueryHandler CreateSut()
        => new(_configProvider, NullLogger<GenerateCliSnippetQueryHandler>.Instance);

    [Fact]
    public async Task Handle_UsesConfiguredEndpointAndRegion()
    {
        // Arrange
        var sut = CreateSut();
        var query = new GenerateCliSnippetQuery("s3api", "list-buckets", []);

        // Act
        var result = await sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Command.Should().Be(
            "aws s3api list-buckets --endpoint-url http://localhost:4566 --region eu-west-1");
    }

    [Fact]
    public async Task Handle_WithSensitiveParameter_NeverEmbedsValue()
    {
        // Arrange
        var sut = CreateSut();
        var query = new GenerateCliSnippetQuery(
            "sts",
            "get-session-token",
            [new CliParameter("token-code", "supersecret", IsSensitive: true)]);

        // Act
        var result = await sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Value.Command.Should().Contain("--token-code <token-code>");
        result.Value.Command.Should().NotContain("supersecret");
    }
}
