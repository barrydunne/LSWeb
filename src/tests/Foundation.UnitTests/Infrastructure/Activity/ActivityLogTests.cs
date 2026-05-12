using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Foundation.Infrastructure.Activity;

namespace Foundation.UnitTests.Infrastructure.Activity;

public class ActivityLogTests
{
    [Fact]
    public void GetEntries_WhenEmpty_ReturnsNoEntries()
    {
        // Arrange
        var sut = new ActivityLog();

        // Act
        var entries = sut.GetEntries();

        // Assert
        entries.Should().BeEmpty();
    }

    [Fact]
    public void Append_WhenInvoked_ReturnsEntriesMostRecentFirst()
    {
        // Arrange
        var sut = new ActivityLog();
        var first = Entry("op-1");
        var second = Entry("op-2");

        // Act
        sut.Append(first);
        sut.Append(second);

        // Assert
        sut.GetEntries().Should().ContainInOrder(second, first);
    }

    [Fact]
    public void Append_WhenCapacityExceeded_EvictsOldestEntry()
    {
        // Arrange
        var sut = new ActivityLog();
        for (var index = 0; index < 101; index++)
        {
            sut.Append(Entry($"op-{index}"));
        }

        // Act
        var entries = sut.GetEntries();

        // Assert
        entries.Should().HaveCount(100);
        entries.Should().NotContain(_ => _.OperationId == "op-0");
        entries[0].OperationId.Should().Be("op-100");
    }

    private static ActivityEntry Entry(string operationId)
        => new(operationId, "catalogue-refresh", OperationState.Succeeded, "Service catalogue refreshed.", DateTimeOffset.UtcNow);
}
