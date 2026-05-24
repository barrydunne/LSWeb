using Amazon.CloudWatchLogs.Model;
using Foundation.Infrastructure.CloudWatchLogs;

namespace Foundation.UnitTests.Infrastructure.CloudWatchLogs;

public class LogEventMapperTests
{
    [Fact]
    public void ToLogEvent_MapsTimestampAndMessage()
    {
        // Arrange
        var timestamp = new DateTime(2024, 7, 8, 9, 10, 11, DateTimeKind.Unspecified);
        var sdk = new OutputLogEvent
        {
            Timestamp = timestamp,
            Message = "hello world",
        };

        // Act
        var logEvent = LogEventMapper.ToLogEvent(sdk);

        // Assert
        logEvent.Timestamp.Should().Be(new DateTimeOffset(timestamp, TimeSpan.Zero));
        logEvent.Message.Should().Be("hello world");
    }

    [Fact]
    public void ToLogEvent_WhenValuesNull_AppliesDefaults()
    {
        // Arrange
        var sdk = new OutputLogEvent();

        // Act
        var logEvent = LogEventMapper.ToLogEvent(sdk);

        // Assert
        logEvent.Timestamp.Should().Be(DateTimeOffset.UnixEpoch);
        logEvent.Message.Should().BeEmpty();
    }

    [Fact]
    public void ToFilteredLogEvent_MapsTimestampAndMessage()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2024, 7, 8, 9, 10, 11, TimeSpan.Zero);
        var sdk = new FilteredLogEvent
        {
            Timestamp = timestamp.ToUnixTimeMilliseconds(),
            Message = "hello world",
        };

        // Act
        var logEvent = LogEventMapper.ToFilteredLogEvent(sdk);

        // Assert
        logEvent.Timestamp.Should().Be(timestamp);
        logEvent.Message.Should().Be("hello world");
    }

    [Fact]
    public void ToFilteredLogEvent_WhenValuesNull_AppliesDefaults()
    {
        // Arrange
        var sdk = new FilteredLogEvent();

        // Act
        var logEvent = LogEventMapper.ToFilteredLogEvent(sdk);

        // Assert
        logEvent.Timestamp.Should().Be(DateTimeOffset.UnixEpoch);
        logEvent.Message.Should().BeEmpty();
    }
}
