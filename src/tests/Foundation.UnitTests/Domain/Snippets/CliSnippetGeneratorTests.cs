using Foundation.Domain.Snippets;

namespace Foundation.UnitTests.Domain.Snippets;

public class CliSnippetGeneratorTests
{
    private static readonly CliConnectionContext _context = new("http://localhost:4566", "eu-west-1");

    [Fact]
    public void Generate_WithNoParameters_RendersServiceOperationAndConnection()
    {
        // Arrange
        var operation = new CliOperation("s3api", "list-buckets", []);

        // Act
        var snippet = CliSnippetGenerator.Generate(operation, _context);

        // Assert
        snippet.Command.Should().Be(
            "aws s3api list-buckets --endpoint-url http://localhost:4566 --region eu-west-1");
    }

    [Fact]
    public void Generate_WithPlainValue_RendersValueUnquoted()
    {
        // Arrange
        var operation = new CliOperation("s3api", "head-bucket", [new CliParameter("bucket", "my-bucket")]);

        // Act
        var snippet = CliSnippetGenerator.Generate(operation, _context);

        // Assert
        snippet.Command.Should().Be(
            "aws s3api head-bucket --bucket my-bucket --endpoint-url http://localhost:4566 --region eu-west-1");
    }

    [Fact]
    public void Generate_WithValueContainingSpace_QuotesValue()
    {
        // Arrange
        var operation = new CliOperation("s3api", "put-object", [new CliParameter("key", "my file.txt")]);

        // Act
        var snippet = CliSnippetGenerator.Generate(operation, _context);

        // Assert
        snippet.Command.Should().Contain("--key \"my file.txt\"");
    }

    [Fact]
    public void Generate_WithSensitiveValue_EmitsPlaceholderAndNeverEmbedsValue()
    {
        // Arrange
        var operation = new CliOperation("sts", "get-session-token", [new CliParameter("token-code", "supersecret", IsSensitive: true)]);

        // Act
        var snippet = CliSnippetGenerator.Generate(operation, _context);

        // Assert
        snippet.Command.Should().Contain("--token-code <token-code>");
        snippet.Command.Should().NotContain("supersecret");
    }

    [Fact]
    public void Generate_WithEmptyValue_EmitsPlaceholder()
    {
        // Arrange
        var operation = new CliOperation("s3api", "get-object", [new CliParameter("key", "   ")]);

        // Act
        var snippet = CliSnippetGenerator.Generate(operation, _context);

        // Assert
        snippet.Command.Should().Contain("--key <key>");
    }

    [Fact]
    public void Generate_WithProfile_AppendsProfileFlag()
    {
        // Arrange
        var context = new CliConnectionContext("http://localhost:4566", "eu-west-1", "localstack");
        var operation = new CliOperation("s3api", "list-buckets", []);

        // Act
        var snippet = CliSnippetGenerator.Generate(operation, context);

        // Assert
        snippet.Command.Should().EndWith("--profile localstack");
    }

    [Fact]
    public void Generate_WithoutProfile_OmitsProfileFlag()
    {
        // Arrange
        var operation = new CliOperation("s3api", "list-buckets", []);

        // Act
        var snippet = CliSnippetGenerator.Generate(operation, _context);

        // Assert
        snippet.Command.Should().NotContain("--profile");
    }

    [Fact]
    public void Generate_WithMultipleParameters_RendersAllInOrder()
    {
        // Arrange
        var operation = new CliOperation("s3api", "put-object", [
            new CliParameter("bucket", "my-bucket"),
            new CliParameter("key", "report.json")
        ]);

        // Act
        var snippet = CliSnippetGenerator.Generate(operation, _context);

        // Assert
        snippet.Command.Should().Be(
            "aws s3api put-object --bucket my-bucket --key report.json --endpoint-url http://localhost:4566 --region eu-west-1");
    }
}
