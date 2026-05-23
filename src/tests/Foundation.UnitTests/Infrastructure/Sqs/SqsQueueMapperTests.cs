using Foundation.Infrastructure.Sqs;

namespace Foundation.UnitTests.Infrastructure.Sqs;

public class SqsQueueMapperTests
{
    [Fact]
    public void ToQueue_DerivesNameFromLastUrlSegment()
    {
        // Arrange
        var attributes = new Dictionary<string, string>
        {
            ["ApproximateNumberOfMessages"] = "5",
            ["ApproximateNumberOfMessagesNotVisible"] = "2",
            ["ApproximateNumberOfMessagesDelayed"] = "1",
        };

        // Act
        var queue = SqsQueueMapper.ToQueue("http://localhost:4566/000000000000/orders", attributes);

        // Assert
        queue.Name.Should().Be("orders");
        queue.Url.Should().Be("http://localhost:4566/000000000000/orders");
        queue.ApproximateMessageCount.Should().Be(5);
        queue.ApproximateInFlightCount.Should().Be(2);
        queue.ApproximateDelayedCount.Should().Be(1);
    }

    [Fact]
    public void ToQueue_WhenAttributesNull_DefaultsCountsToZero()
    {
        // Act
        var queue = SqsQueueMapper.ToQueue("http://localhost:4566/000000000000/orders", null);

        // Assert
        queue.Name.Should().Be("orders");
        queue.ApproximateMessageCount.Should().Be(0);
        queue.ApproximateInFlightCount.Should().Be(0);
        queue.ApproximateDelayedCount.Should().Be(0);
    }

    [Fact]
    public void ToQueue_WhenAttributeMissingOrUnparseable_DefaultsCountToZero()
    {
        // Arrange
        var attributes = new Dictionary<string, string>
        {
            ["ApproximateNumberOfMessages"] = "not-a-number",
        };

        // Act
        var queue = SqsQueueMapper.ToQueue("http://localhost:4566/000000000000/orders", attributes);

        // Assert
        queue.ApproximateMessageCount.Should().Be(0);
        queue.ApproximateInFlightCount.Should().Be(0);
        queue.ApproximateDelayedCount.Should().Be(0);
    }

    [Fact]
    public void ToQueue_WhenUrlHasNoSlash_UsesWholeUrlAsName()
    {
        // Act
        var queue = SqsQueueMapper.ToQueue("orders", null);

        // Assert
        queue.Name.Should().Be("orders");
    }

    [Fact]
    public void ToQueue_WhenUrlEndsWithSlash_UsesWholeUrlAsName()
    {
        // Act
        var queue = SqsQueueMapper.ToQueue("http://localhost:4566/000000000000/", null);

        // Assert
        queue.Name.Should().Be("http://localhost:4566/000000000000/");
    }
}
