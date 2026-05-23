using Foundation.Infrastructure.Sqs;

namespace Foundation.UnitTests.Infrastructure.Sqs;

public class SqsRedriveMapperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseRedrive_WhenRedrivePolicyMissing_ReturnsNullTarget(string? policy)
    {
        // Act
        var result = SqsRedriveMapper.ParseRedrive(policy, null);

        // Assert
        result.DeadLetterTarget.Should().BeNull();
    }

    [Fact]
    public void ParseRedrive_WhenRedrivePolicyNotValidJson_ReturnsNullTarget()
    {
        // Act
        var result = SqsRedriveMapper.ParseRedrive("{ not json", null);

        // Assert
        result.DeadLetterTarget.Should().BeNull();
    }

    [Fact]
    public void ParseRedrive_WhenRedrivePolicyIsNotObject_ReturnsNullTarget()
    {
        // Act
        var result = SqsRedriveMapper.ParseRedrive("[]", null);

        // Assert
        result.DeadLetterTarget.Should().BeNull();
    }

    [Fact]
    public void ParseRedrive_WhenDeadLetterTargetArnMissing_ReturnsNullTarget()
    {
        // Arrange
        const string policy = """{ "maxReceiveCount": 5 }""";

        // Act
        var result = SqsRedriveMapper.ParseRedrive(policy, null);

        // Assert
        result.DeadLetterTarget.Should().BeNull();
    }

    [Fact]
    public void ParseRedrive_WhenDeadLetterTargetArnNotString_ReturnsNullTarget()
    {
        // Arrange
        const string policy = """{ "deadLetterTargetArn": 123 }""";

        // Act
        var result = SqsRedriveMapper.ParseRedrive(policy, null);

        // Assert
        result.DeadLetterTarget.Should().BeNull();
    }

    [Fact]
    public void ParseRedrive_WhenRedrivePolicyValid_ReturnsTarget()
    {
        // Arrange
        const string policy = """
        {
          "deadLetterTargetArn": "arn:aws:sqs:eu-west-1:000000000000:orders-dlq",
          "maxReceiveCount": 5
        }
        """;

        // Act
        var result = SqsRedriveMapper.ParseRedrive(policy, null);

        // Assert
        result.DeadLetterTarget.Should().NotBeNull();
        result.DeadLetterTarget!.QueueArn.Should().Be("arn:aws:sqs:eu-west-1:000000000000:orders-dlq");
        result.DeadLetterTarget.QueueName.Should().Be("orders-dlq");
        result.DeadLetterTarget.MaxReceiveCount.Should().Be(5);
    }

    [Fact]
    public void ParseRedrive_WhenMaxReceiveCountIsString_IsParsed()
    {
        // Arrange
        const string policy = """
        {
          "deadLetterTargetArn": "arn:aws:sqs:eu-west-1:000000000000:orders-dlq",
          "maxReceiveCount": "10"
        }
        """;

        // Act
        var result = SqsRedriveMapper.ParseRedrive(policy, null);

        // Assert
        result.DeadLetterTarget!.MaxReceiveCount.Should().Be(10);
    }

    [Fact]
    public void ParseRedrive_WhenMaxReceiveCountMissing_DefaultsToZero()
    {
        // Arrange
        const string policy = """{ "deadLetterTargetArn": "arn:aws:sqs:eu-west-1:000000000000:orders-dlq" }""";

        // Act
        var result = SqsRedriveMapper.ParseRedrive(policy, null);

        // Assert
        result.DeadLetterTarget!.MaxReceiveCount.Should().Be(0);
    }

    [Fact]
    public void ParseRedrive_WhenMaxReceiveCountUnparseable_DefaultsToZero()
    {
        // Arrange
        const string policy = """
        {
          "deadLetterTargetArn": "arn:aws:sqs:eu-west-1:000000000000:orders-dlq",
          "maxReceiveCount": "not-a-number"
        }
        """;

        // Act
        var result = SqsRedriveMapper.ParseRedrive(policy, null);

        // Assert
        result.DeadLetterTarget!.MaxReceiveCount.Should().Be(0);
    }

    [Fact]
    public void ParseRedrive_WhenMaxReceiveCountIsBool_DefaultsToZero()
    {
        // Arrange
        const string policy = """
        {
          "deadLetterTargetArn": "arn:aws:sqs:eu-west-1:000000000000:orders-dlq",
          "maxReceiveCount": true
        }
        """;

        // Act
        var result = SqsRedriveMapper.ParseRedrive(policy, null);

        // Assert
        result.DeadLetterTarget!.MaxReceiveCount.Should().Be(0);
    }

    [Fact]
    public void ParseRedrive_WhenTargetArnIsNotAnArn_FallsBackToArnAsName()
    {
        // Arrange
        const string policy = """{ "deadLetterTargetArn": "not-an-arn", "maxReceiveCount": 3 }""";

        // Act
        var result = SqsRedriveMapper.ParseRedrive(policy, null);

        // Assert
        result.DeadLetterTarget!.QueueName.Should().Be("not-an-arn");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseRedrive_WhenRedriveAllowPolicyMissing_ReturnsEmptySources(string? policy)
    {
        // Act
        var result = SqsRedriveMapper.ParseRedrive(null, policy);

        // Assert
        result.Sources.Should().BeEmpty();
    }

    [Fact]
    public void ParseRedrive_WhenRedriveAllowPolicyNotValidJson_ReturnsEmptySources()
    {
        // Act
        var result = SqsRedriveMapper.ParseRedrive(null, "{ not json");

        // Assert
        result.Sources.Should().BeEmpty();
    }

    [Fact]
    public void ParseRedrive_WhenRedriveAllowPolicyIsNotObject_ReturnsEmptySources()
    {
        // Act
        var result = SqsRedriveMapper.ParseRedrive(null, "[]");

        // Assert
        result.Sources.Should().BeEmpty();
    }

    [Fact]
    public void ParseRedrive_WhenSourceQueueArnsMissing_ReturnsEmptySources()
    {
        // Arrange
        const string policy = """{ "redrivePermission": "byQueue" }""";

        // Act
        var result = SqsRedriveMapper.ParseRedrive(null, policy);

        // Assert
        result.Sources.Should().BeEmpty();
    }

    [Fact]
    public void ParseRedrive_WhenSourceQueueArnsNotArray_ReturnsEmptySources()
    {
        // Arrange
        const string policy = """{ "sourceQueueArns": "arn:aws:sqs:eu-west-1:000000000000:orders" }""";

        // Act
        var result = SqsRedriveMapper.ParseRedrive(null, policy);

        // Assert
        result.Sources.Should().BeEmpty();
    }

    [Fact]
    public void ParseRedrive_WhenSourceQueueArnsValid_ReturnsSources()
    {
        // Arrange
        const string policy = """
        {
          "redrivePermission": "byQueue",
          "sourceQueueArns": [
            "arn:aws:sqs:eu-west-1:000000000000:orders",
            "arn:aws:sqs:eu-west-1:000000000000:payments"
          ]
        }
        """;

        // Act
        var result = SqsRedriveMapper.ParseRedrive(null, policy);

        // Assert
        result.Sources.Select(source => source.QueueName).Should().Equal("orders", "payments");
    }

    [Fact]
    public void ParseRedrive_WhenSourceQueueArnsContainNonString_SkipsThem()
    {
        // Arrange
        const string policy = """
        {
          "sourceQueueArns": [
            "arn:aws:sqs:eu-west-1:000000000000:orders",
            12345
          ]
        }
        """;

        // Act
        var result = SqsRedriveMapper.ParseRedrive(null, policy);

        // Assert
        result.Sources.Select(source => source.QueueName).Should().Equal("orders");
    }

    [Fact]
    public void ParseRedrive_WhenSourceArnDuplicated_IsDeduplicated()
    {
        // Arrange
        const string policy = """
        {
          "sourceQueueArns": [
            "arn:aws:sqs:eu-west-1:000000000000:orders",
            "arn:aws:sqs:eu-west-1:000000000000:orders"
          ]
        }
        """;

        // Act
        var result = SqsRedriveMapper.ParseRedrive(null, policy);

        // Assert
        result.Sources.Should().ContainSingle().Which.QueueName.Should().Be("orders");
    }

    [Fact]
    public void ParseRedrive_WhenSourceArnIsNotAnArn_FallsBackToArnAsName()
    {
        // Arrange
        const string policy = """{ "sourceQueueArns": [ "not-an-arn" ] }""";

        // Act
        var result = SqsRedriveMapper.ParseRedrive(null, policy);

        // Assert
        result.Sources.Should().ContainSingle().Which.QueueName.Should().Be("not-an-arn");
    }
}
