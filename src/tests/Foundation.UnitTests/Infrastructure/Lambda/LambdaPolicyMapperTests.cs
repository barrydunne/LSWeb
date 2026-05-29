using Foundation.Infrastructure.Lambda;

namespace Foundation.UnitTests.Infrastructure.Lambda;

public class LambdaPolicyMapperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseS3Triggers_WhenPolicyMissing_ReturnsEmpty(string? policy)
    {
        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseS3Triggers_WhenPolicyNotValidJson_ReturnsEmpty()
    {
        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers("{ not json");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseS3Triggers_WhenNoStatementProperty_ReturnsEmpty()
    {
        // Arrange
        const string policy = """{ "Version": "2012-10-17" }""";

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseS3Triggers_WhenSingleStatementWithS3Source_ReturnsTrigger()
    {
        // Arrange
        const string policy = """
        {
          "Version": "2012-10-17",
          "Statement": {
            "Effect": "Allow",
            "Principal": { "Service": "s3.amazonaws.com" },
            "Action": "lambda:InvokeFunction",
            "Resource": "arn:aws:lambda:eu-west-1:000000000000:function:process-orders",
            "Condition": {
              "ArnLike": { "AWS:SourceArn": "arn:aws:s3:::orders-bucket" }
            }
          }
        }
        """;

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().ContainSingle().Which.BucketArn.Should().Be("arn:aws:s3:::orders-bucket");
    }

    [Fact]
    public void ParseS3Triggers_WhenStatementArrayWithMultipleSources_ReturnsAll()
    {
        // Arrange
        const string policy = """
        {
          "Statement": [
            {
              "Condition": {
                "ArnLike": { "AWS:SourceArn": "arn:aws:s3:::bucket-a" }
              }
            },
            {
              "Condition": {
                "ArnEquals": { "AWS:SourceArn": "arn:aws:s3:::bucket-b" }
              }
            }
          ]
        }
        """;

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Select(trigger => trigger.BucketArn).Should().Equal("arn:aws:s3:::bucket-a", "arn:aws:s3:::bucket-b");
    }

    [Fact]
    public void ParseS3Triggers_WhenSourceArnIsArray_ReturnsEachBucket()
    {
        // Arrange
        const string policy = """
        {
          "Statement": {
            "Condition": {
              "ArnLike": {
                "AWS:SourceArn": [
                  "arn:aws:s3:::bucket-a",
                  "arn:aws:s3:::bucket-b",
                  12345
                ]
              }
            }
          }
        }
        """;

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Select(trigger => trigger.BucketArn).Should().Equal("arn:aws:s3:::bucket-a", "arn:aws:s3:::bucket-b");
    }

    [Fact]
    public void ParseS3Triggers_WhenSourceIsNotS3_IsIgnored()
    {
        // Arrange
        const string policy = """
        {
          "Statement": {
            "Condition": {
              "ArnEquals": { "AWS:SourceArn": "arn:aws:sns:eu-west-1:000000000000:order-events" }
            }
          }
        }
        """;

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseS3Triggers_WhenSourceArnIsNotAnArn_IsIgnored()
    {
        // Arrange
        const string policy = """
        {
          "Statement": {
            "Condition": {
              "ArnEquals": { "AWS:SourceArn": "not-an-arn" }
            }
          }
        }
        """;

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseS3Triggers_WhenSameBucketAppearsTwice_IsDeduplicated()
    {
        // Arrange
        const string policy = """
        {
          "Statement": [
            { "Condition": { "ArnEquals": { "AWS:SourceArn": "arn:aws:s3:::dup" } } },
            { "Condition": { "ArnLike": { "aws:SourceArn": "arn:aws:s3:::dup" } } }
          ]
        }
        """;

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().ContainSingle().Which.BucketArn.Should().Be("arn:aws:s3:::dup");
    }

    [Fact]
    public void ParseS3Triggers_WhenStatementIsNotObject_IsSkipped()
    {
        // Arrange
        const string policy = """{ "Statement": [ "not-an-object" ] }""";

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseS3Triggers_WhenStatementHasNoCondition_IsSkipped()
    {
        // Arrange
        const string policy = """{ "Statement": { "Effect": "Allow" } }""";

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseS3Triggers_WhenConditionIsNotObject_IsSkipped()
    {
        // Arrange
        const string policy = """{ "Statement": { "Condition": "nope" } }""";

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseS3Triggers_WhenConditionOperatorIsNotObject_IsSkipped()
    {
        // Arrange
        const string policy = """{ "Statement": { "Condition": { "ArnLike": "nope" } } }""";

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseS3Triggers_WhenConditionKeyIsNotSourceArn_IsSkipped()
    {
        // Arrange
        const string policy = """
        {
          "Statement": {
            "Condition": {
              "StringEquals": { "AWS:SourceAccount": "000000000000" }
            }
          }
        }
        """;

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseS3Triggers_WhenSourceArnIsNotStringOrArray_IsSkipped()
    {
        // Arrange
        const string policy = """{ "Statement": { "Condition": { "ArnLike": { "AWS:SourceArn": 12345 } } } }""";

        // Act
        var result = LambdaPolicyMapper.ParseS3Triggers(policy);

        // Assert
        result.Should().BeEmpty();
    }
}
