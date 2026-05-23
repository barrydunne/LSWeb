using Foundation.Infrastructure.Sqs;

namespace Foundation.UnitTests.Infrastructure.Sqs;

public class SqsQueueAttributesMapperTests
{
    [Fact]
    public void ToAttributes_WhenAllValuesPresent_MapsEveryField()
    {
        // Arrange
        var attributes = new Dictionary<string, string>
        {
            ["VisibilityTimeout"] = "45",
            ["MessageRetentionPeriod"] = "86400",
            ["DelaySeconds"] = "10",
            ["ReceiveMessageWaitTimeSeconds"] = "5",
            ["MaximumMessageSize"] = "262144",
            ["QueueArn"] = "arn:aws:sqs:eu-west-1:000000000000:orders",
            ["FifoQueue"] = "true",
        };

        // Act
        var result = SqsQueueAttributesMapper.ToAttributes(attributes);

        // Assert
        result.VisibilityTimeoutSeconds.Should().Be(45);
        result.MessageRetentionPeriodSeconds.Should().Be(86400);
        result.DelaySeconds.Should().Be(10);
        result.ReceiveMessageWaitTimeSeconds.Should().Be(5);
        result.MaximumMessageSizeBytes.Should().Be(262144);
        result.QueueArn.Should().Be("arn:aws:sqs:eu-west-1:000000000000:orders");
        result.FifoQueue.Should().BeTrue();
    }

    [Fact]
    public void ToAttributes_WhenAttributesNull_AppliesDefaults()
    {
        // Act
        var result = SqsQueueAttributesMapper.ToAttributes(null);

        // Assert
        result.VisibilityTimeoutSeconds.Should().Be(0);
        result.MessageRetentionPeriodSeconds.Should().Be(0);
        result.DelaySeconds.Should().Be(0);
        result.ReceiveMessageWaitTimeSeconds.Should().Be(0);
        result.MaximumMessageSizeBytes.Should().Be(0);
        result.QueueArn.Should().BeEmpty();
        result.FifoQueue.Should().BeFalse();
    }

    [Fact]
    public void ToAttributes_WhenIntegerUnparseable_DefaultsToZero()
    {
        // Arrange
        var attributes = new Dictionary<string, string>
        {
            ["VisibilityTimeout"] = "not-a-number",
        };

        // Act
        var result = SqsQueueAttributesMapper.ToAttributes(attributes);

        // Assert
        result.VisibilityTimeoutSeconds.Should().Be(0);
    }

    [Fact]
    public void ToAttributes_WhenFifoFalse_MapsToFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, string>
        {
            ["FifoQueue"] = "false",
        };

        // Act
        var result = SqsQueueAttributesMapper.ToAttributes(attributes);

        // Assert
        result.FifoQueue.Should().BeFalse();
    }

    [Fact]
    public void ToAttributes_WhenFifoUnparseable_DefaultsToFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, string>
        {
            ["FifoQueue"] = "maybe",
        };

        // Act
        var result = SqsQueueAttributesMapper.ToAttributes(attributes);

        // Assert
        result.FifoQueue.Should().BeFalse();
    }
}
