using Foundation.Domain.Streaming;

namespace Foundation.UnitTests.Domain.Streaming;

public class NotificationTests
{
    [Fact]
    public void Constructor_ExposesAllProperties()
    {
        var occurredAt = DateTimeOffset.UtcNow;

        var notification = new Notification("op-1", "catalogue-refresh", OperationState.Succeeded, "Done.", occurredAt);

        notification.OperationId.Should().Be("op-1");
        notification.Operation.Should().Be("catalogue-refresh");
        notification.State.Should().Be(OperationState.Succeeded);
        notification.Message.Should().Be("Done.");
        notification.OccurredAt.Should().Be(occurredAt);
    }
}
