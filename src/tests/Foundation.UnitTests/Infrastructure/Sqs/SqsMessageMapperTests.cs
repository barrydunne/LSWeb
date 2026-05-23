using Amazon.SQS.Model;
using Foundation.Infrastructure.Sqs;

namespace Foundation.UnitTests.Infrastructure.Sqs;

public class SqsMessageMapperTests
{
    [Fact]
    public void ToMessage_MapsCoreFieldsAndAttributes()
    {
        // Arrange
        var message = new Message
        {
            MessageId = "id-1",
            ReceiptHandle = "receipt-1",
            Body = "{\"hello\":\"world\"}",
            Attributes = new Dictionary<string, string>
            {
                ["SentTimestamp"] = "1700000000000",
            },
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["trace"] = new() { DataType = "String", StringValue = "abc" },
            },
        };

        // Act
        var result = SqsMessageMapper.ToMessage(message);

        // Assert
        result.MessageId.Should().Be("id-1");
        result.ReceiptHandle.Should().Be("receipt-1");
        result.Body.Should().Be("{\"hello\":\"world\"}");
        result.Attributes.Should().ContainKey("SentTimestamp").WhoseValue.Should().Be("1700000000000");
        result.MessageAttributes.Should().ContainKey("trace").WhoseValue.Should().Be("abc");
    }

    [Fact]
    public void ToMessage_WhenCollectionsNull_DefaultsToEmptyAndBlankStrings()
    {
        // Arrange
        var message = new Message();

        // Act
        var result = SqsMessageMapper.ToMessage(message);

        // Assert
        result.MessageId.Should().BeEmpty();
        result.ReceiptHandle.Should().BeEmpty();
        result.Body.Should().BeEmpty();
        result.Attributes.Should().BeEmpty();
        result.MessageAttributes.Should().BeEmpty();
    }

    [Fact]
    public void ToMessage_WhenMessageAttributeHasNoStringValue_FallsBackToDataType()
    {
        // Arrange
        var message = new Message
        {
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["binary"] = new() { DataType = "Binary" },
            },
        };

        // Act
        var result = SqsMessageMapper.ToMessage(message);

        // Assert
        result.MessageAttributes.Should().ContainKey("binary").WhoseValue.Should().Be("Binary");
    }

    [Fact]
    public void ToMessage_WhenMessageAttributeHasNoStringValueOrDataType_FallsBackToEmpty()
    {
        // Arrange
        var message = new Message
        {
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["empty"] = new(),
            },
        };

        // Act
        var result = SqsMessageMapper.ToMessage(message);

        // Assert
        result.MessageAttributes.Should().ContainKey("empty").WhoseValue.Should().BeEmpty();
    }
}
