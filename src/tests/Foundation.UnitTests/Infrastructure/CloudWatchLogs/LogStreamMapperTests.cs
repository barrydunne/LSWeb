using Foundation.Infrastructure.CloudWatchLogs;
using SdkLogStream = Amazon.CloudWatchLogs.Model.LogStream;

namespace Foundation.UnitTests.Infrastructure.CloudWatchLogs;

public class LogStreamMapperTests
{
    [Fact]
    public void ToLogStream_MapsNameAndTimestamp()
    {
        // Arrange
        var lastEvent = new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Unspecified);
        var sdk = new SdkLogStream
        {
            LogStreamName = "2024/05/06/[$LATEST]abc",
            LastEventTimestamp = lastEvent,
        };

        // Act
        var stream = LogStreamMapper.ToLogStream(sdk);

        // Assert
        stream.Name.Should().Be("2024/05/06/[$LATEST]abc");
        stream.LastEventTimestamp.Should().Be(new DateTimeOffset(lastEvent, TimeSpan.Zero));
    }

    [Fact]
    public void ToLogStream_WhenValuesNull_AppliesDefaults()
    {
        // Arrange
        var sdk = new SdkLogStream();

        // Act
        var stream = LogStreamMapper.ToLogStream(sdk);

        // Assert
        stream.Name.Should().BeEmpty();
        stream.LastEventTimestamp.Should().BeNull();
    }
}
