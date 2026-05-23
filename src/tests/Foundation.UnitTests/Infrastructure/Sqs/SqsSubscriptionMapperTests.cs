using Foundation.Infrastructure.Sqs;

namespace Foundation.UnitTests.Infrastructure.Sqs;

public class SqsSubscriptionMapperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseSubscriptions_WhenPolicyMissing_ReturnsEmpty(string? policy)
    {
        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSubscriptions_WhenPolicyNotValidJson_ReturnsEmpty()
    {
        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions("{ not json");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSubscriptions_WhenNoStatementProperty_ReturnsEmpty()
    {
        // Arrange
        const string policy = """{ "Version": "2012-10-17" }""";

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSubscriptions_WhenSingleStatementWithSnsSource_ReturnsSubscription()
    {
        // Arrange
        const string policy = """
        {
          "Version": "2012-10-17",
          "Statement": {
            "Effect": "Allow",
            "Principal": { "Service": "sns.amazonaws.com" },
            "Action": "sqs:SendMessage",
            "Resource": "arn:aws:sqs:eu-west-1:000000000000:orders",
            "Condition": {
              "ArnEquals": { "aws:SourceArn": "arn:aws:sns:eu-west-1:000000000000:order-events" }
            }
          }
        }
        """;

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        var subscription = result.Should().ContainSingle().Subject;
        subscription.TopicArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:order-events");
        subscription.TopicName.Should().Be("order-events");
    }

    [Fact]
    public void ParseSubscriptions_WhenStatementArrayWithMultipleSources_ReturnsAll()
    {
        // Arrange
        const string policy = """
        {
          "Statement": [
            {
              "Condition": {
                "ArnEquals": { "aws:SourceArn": "arn:aws:sns:eu-west-1:000000000000:topic-a" }
              }
            },
            {
              "Condition": {
                "ArnLike": { "aws:SourceArn": "arn:aws:sns:eu-west-1:000000000000:topic-b" }
              }
            }
          ]
        }
        """;

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Select(subscription => subscription.TopicName).Should().Equal("topic-a", "topic-b");
    }

    [Fact]
    public void ParseSubscriptions_WhenSourceArnIsArray_ReturnsEachSnsTopic()
    {
        // Arrange
        const string policy = """
        {
          "Statement": {
            "Condition": {
              "ArnEquals": {
                "aws:SourceArn": [
                  "arn:aws:sns:eu-west-1:000000000000:topic-a",
                  "arn:aws:sns:eu-west-1:000000000000:topic-b",
                  12345
                ]
              }
            }
          }
        }
        """;

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Select(subscription => subscription.TopicName).Should().Equal("topic-a", "topic-b");
    }

    [Fact]
    public void ParseSubscriptions_WhenSourceIsNotSns_IsIgnored()
    {
        // Arrange
        const string policy = """
        {
          "Statement": {
            "Condition": {
              "ArnEquals": { "aws:SourceArn": "arn:aws:s3:::my-bucket" }
            }
          }
        }
        """;

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSubscriptions_WhenSourceArnIsNotAnArn_IsIgnored()
    {
        // Arrange
        const string policy = """
        {
          "Statement": {
            "Condition": {
              "ArnEquals": { "aws:SourceArn": "not-an-arn" }
            }
          }
        }
        """;

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSubscriptions_WhenSameTopicAppearsTwice_IsDeduplicated()
    {
        // Arrange
        const string policy = """
        {
          "Statement": [
            { "Condition": { "ArnEquals": { "aws:SourceArn": "arn:aws:sns:eu-west-1:000000000000:dup" } } },
            { "Condition": { "ArnLike": { "AWS:SourceArn": "arn:aws:sns:eu-west-1:000000000000:dup" } } }
          ]
        }
        """;

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().ContainSingle().Which.TopicName.Should().Be("dup");
    }

    [Fact]
    public void ParseSubscriptions_WhenStatementIsNotObject_IsSkipped()
    {
        // Arrange
        const string policy = """{ "Statement": [ "not-an-object" ] }""";

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSubscriptions_WhenStatementHasNoCondition_IsSkipped()
    {
        // Arrange
        const string policy = """{ "Statement": { "Effect": "Allow" } }""";

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSubscriptions_WhenConditionIsNotObject_IsSkipped()
    {
        // Arrange
        const string policy = """{ "Statement": { "Condition": "nope" } }""";

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSubscriptions_WhenConditionOperatorIsNotObject_IsSkipped()
    {
        // Arrange
        const string policy = """{ "Statement": { "Condition": { "ArnEquals": "nope" } } }""";

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSubscriptions_WhenConditionKeyIsNotSourceArn_IsSkipped()
    {
        // Arrange
        const string policy = """
        {
          "Statement": {
            "Condition": {
              "StringEquals": { "aws:SourceAccount": "000000000000" }
            }
          }
        }
        """;

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseSubscriptions_WhenSourceArnIsNotStringOrArray_IsSkipped()
    {
        // Arrange
        const string policy = """{ "Statement": { "Condition": { "ArnEquals": { "aws:SourceArn": 12345 } } } }""";

        // Act
        var result = SqsSubscriptionMapper.ParseSubscriptions(policy);

        // Assert
        result.Should().BeEmpty();
    }
}
