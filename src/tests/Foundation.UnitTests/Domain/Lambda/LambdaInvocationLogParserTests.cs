using Foundation.Domain.Lambda;

namespace Foundation.UnitTests.Domain.Lambda;

public class LambdaInvocationLogParserTests
{
    [Fact]
    public void Parse_WhenNoEvents_ReturnsZeroMetricsAndNoInvocations()
    {
        // Act
        var result = LambdaInvocationLogParser.Parse([]);

        // Assert
        result.Metrics.Should().Be(new LambdaInvocationMetrics(0, 0, 0, 0));
        result.RecentInvocations.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhenEventHasNoRequestId_IgnoresEvent()
    {
        // Arrange
        IReadOnlyList<LambdaLogEvent> events =
        [
            new("2026-01-01T00:00:00.0000000+00:00", "INIT_START Runtime Version: dotnet", "stream-a"),
        ];

        // Act
        var result = LambdaInvocationLogParser.Parse(events);

        // Assert
        result.Metrics.InvocationCount.Should().Be(0);
        result.RecentInvocations.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhenRequestIdMarkerHasNoToken_IgnoresEvent()
    {
        // Arrange
        IReadOnlyList<LambdaLogEvent> events =
        [
            new("2026-01-01T00:00:00.0000000+00:00", "RequestId: ", "stream-a"),
        ];

        // Act
        var result = LambdaInvocationLogParser.Parse(events);

        // Assert
        result.Metrics.InvocationCount.Should().Be(0);
        result.RecentInvocations.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhenInvocationHasOnlyStart_ExcludesIncompleteInvocation()
    {
        // Arrange
        IReadOnlyList<LambdaLogEvent> events =
        [
            new("2026-01-01T00:00:00.0000000+00:00", "START RequestId: abc Version: $LATEST", "stream-a"),
        ];

        // Act
        var result = LambdaInvocationLogParser.Parse(events);

        // Assert
        result.Metrics.InvocationCount.Should().Be(0);
        result.RecentInvocations.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhenDurationIsNotANumber_TreatsInvocationAsIncomplete()
    {
        // Arrange
        IReadOnlyList<LambdaLogEvent> events =
        [
            new("2026-01-01T00:00:00.0000000+00:00", "REPORT RequestId: abc Duration: abc ms", "stream-a"),
        ];

        // Act
        var result = LambdaInvocationLogParser.Parse(events);

        // Assert
        result.Metrics.InvocationCount.Should().Be(0);
        result.RecentInvocations.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhenInvocationCompletes_DerivesDurationAndTimestamp()
    {
        // Arrange
        IReadOnlyList<LambdaLogEvent> events =
        [
            new("2026-01-01T00:00:00.0000000+00:00", "START RequestId: abc Version: $LATEST", "stream-a"),
            new("2026-01-01T00:00:01.0000000+00:00", "REPORT RequestId: abc Duration: 12.50 ms Billed Duration: 13 ms", "stream-a"),
        ];

        // Act
        var result = LambdaInvocationLogParser.Parse(events);

        // Assert
        result.Metrics.Should().Be(new LambdaInvocationMetrics(1, 0, 12.50, 12.50));
        var invocation = result.RecentInvocations.Should().ContainSingle().Subject;
        invocation.RequestId.Should().Be("abc");
        invocation.Timestamp.Should().Be("2026-01-01T00:00:01.0000000+00:00");
        invocation.DurationMs.Should().Be(12.50);
        invocation.HasError.Should().BeFalse();
    }

    [Fact]
    public void Parse_WhenInvocationLogsError_MarksInvocationAsError()
    {
        // Arrange
        IReadOnlyList<LambdaLogEvent> events =
        [
            new("2026-01-01T00:00:00.0000000+00:00", "[ERROR] RequestId: abc Something failed", "stream-a"),
            new("2026-01-01T00:00:01.0000000+00:00", "REPORT RequestId: abc Duration: 5.00 ms", "stream-a"),
        ];

        // Act
        var result = LambdaInvocationLogParser.Parse(events);

        // Assert
        result.Metrics.Should().Be(new LambdaInvocationMetrics(1, 1, 5.00, 5.00));
        result.RecentInvocations.Should().ContainSingle().Which.HasError.Should().BeTrue();
    }

    [Fact]
    public void Parse_WhenMultipleInvocations_AggregatesMetricsAndOrdersNewestFirst()
    {
        // Arrange
        IReadOnlyList<LambdaLogEvent> events =
        [
            new("2026-01-01T00:00:01.0000000+00:00", "REPORT RequestId: first Duration: 10.00 ms", "stream-a"),
            new("2026-01-01T00:00:02.0000000+00:00", "[ERROR] RequestId: second Task timed out after 3.00 seconds", "stream-b"),
            new("2026-01-01T00:00:03.0000000+00:00", "REPORT RequestId: second Duration: 30.00 ms", "stream-b"),
        ];

        // Act
        var result = LambdaInvocationLogParser.Parse(events);

        // Assert
        result.Metrics.InvocationCount.Should().Be(2);
        result.Metrics.ErrorCount.Should().Be(1);
        result.Metrics.AverageDurationMs.Should().Be(20.00);
        result.Metrics.MaxDurationMs.Should().Be(30.00);
        result.RecentInvocations.Select(_ => _.RequestId).Should().ContainInOrder("second", "first");
    }
}
