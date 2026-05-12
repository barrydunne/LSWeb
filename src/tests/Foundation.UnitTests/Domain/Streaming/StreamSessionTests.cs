using Foundation.Domain.Streaming;

namespace Foundation.UnitTests.Domain.Streaming;

public class StreamSessionTests
{
    [Fact]
    public void Constructor_ExposesAllProperties()
    {
        var connectedAt = DateTimeOffset.UtcNow;

        var session = new StreamSession("conn-1", connectedAt);

        session.ConnectionId.Should().Be("conn-1");
        session.ConnectedAt.Should().Be(connectedAt);
    }
}
