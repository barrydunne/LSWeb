using Foundation.Domain.Navigation;
using Foundation.Infrastructure.Navigation;

namespace Foundation.UnitTests.Infrastructure.Navigation;

public class ReferenceResolverTests
{
    private readonly ReferenceResolver _sut = new();

    [Theory]
    [InlineData("arn:aws:sqs:eu-west-1:000000000000:my-queue", "sqs", "my-queue", "/services/sqs/my-queue")]
    [InlineData("arn:aws:sns:eu-west-1:000000000000:my-topic", "sns", "my-topic", "/services/sns/my-topic")]
    [InlineData("arn:aws:lambda:eu-west-1:000000000000:function:my-func", "lambda", "my-func", "/services/lambda/my-func")]
    [InlineData("arn:aws:s3:::my-bucket", "s3", "my-bucket", "/services/s3/my-bucket")]
    [InlineData("arn:aws:dynamodb:eu-west-1:000000000000:table/my-table", "dynamodb", "my-table", "/services/dynamodb/my-table")]
    [InlineData("arn:aws:logs:eu-west-1:000000000000:log-group:my-group", "cloudwatch-logs", "my-group", "/services/cloudwatch-logs/my-group")]
    [InlineData("arn:aws:secretsmanager:eu-west-1:000000000000:secret:my-secret", "secrets-manager", "my-secret", "/services/secrets-manager/my-secret")]
    [InlineData("arn:aws:ssm:eu-west-1:000000000000:parameter/my-param", "ssm-parameter-store", "my-param", "/services/ssm-parameter-store/my-param")]
    [InlineData("arn:aws:states:eu-west-1:000000000000:stateMachine:my-machine", "step-functions", "my-machine", "/services/step-functions/my-machine")]
    public void Resolve_WhenArnForKnownService_ReturnsReference(
        string reference,
        string expectedKey,
        string expectedResourceId,
        string expectedRoute)
    {
        var result = _sut.Resolve(reference);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new ResourceReference(expectedKey, expectedResourceId, expectedRoute));
    }

    [Fact]
    public void Resolve_WhenResourceContainsReservedCharacters_EscapesTheRouteSegment()
    {
        var result = _sut.Resolve("arn:aws:sqs:eu-west-1:000000000000:my queue");

        result.IsSuccess.Should().BeTrue();
        result.Value.ResourceId.Should().Be("my queue");
        result.Value.Route.Should().Be("/services/sqs/my%20queue");
    }

    [Fact]
    public void Resolve_WhenBareIdQualifiedByService_ReturnsReference()
    {
        var result = _sut.Resolve("my-queue", "sqs");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new ResourceReference("sqs", "my-queue", "/services/sqs/my-queue"));
    }

    [Fact]
    public void Resolve_WhenBareIdQualifiedByCatalogueKey_ReturnsReference()
    {
        var result = _sut.Resolve("my-group", "cloudwatch-logs");

        result.IsSuccess.Should().BeTrue();
        result.Value.ServiceKey.Should().Be("cloudwatch-logs");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WhenReferenceIsBlank_ReturnsError(string? reference)
    {
        var result = _sut.Resolve(reference!);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("A resource reference is required.");
    }

    [Fact]
    public void Resolve_WhenReferenceLooksLikeArnButIsMalformed_ReturnsError()
    {
        var result = _sut.Resolve("arn:aws:sqs");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("'arn:aws:sqs' is not a valid ARN.");
    }

    [Fact]
    public void Resolve_WhenBareIdHasNoService_ReturnsError()
    {
        var result = _sut.Resolve("my-queue");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("A service is required to resolve a non-ARN reference.");
    }

    [Fact]
    public void Resolve_WhenArnServiceIsUnsupported_ReturnsError()
    {
        var result = _sut.Resolve("arn:aws:ec2:eu-west-1:000000000000:instance/i-123");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("Unsupported service 'ec2'.");
    }

    [Fact]
    public void Resolve_WhenBareServiceIsUnsupported_ReturnsError()
    {
        var result = _sut.Resolve("i-123", "ec2");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("Unsupported service 'ec2'.");
    }
}
