using Foundation.Domain.Seed;
using Foundation.Domain.Streaming;

namespace Foundation.UnitTests.Domain.Seed;

public class SeedOutcomeTests
{
    [Fact]
    public void Computed_WhenAllResourcesSucceeded_ReportsSucceeded()
    {
        // Arrange
        var outcome = new SeedOutcome(
            "op-1",
            "messaging-starter",
            [
                new SeedResourceResult("sqs", "Queue", "q", true, null),
                new SeedResourceResult("sns", "Topic", "t", true, null),
            ]);

        // Assert
        outcome.TotalCount.Should().Be(2);
        outcome.SucceededCount.Should().Be(2);
        outcome.FailedCount.Should().Be(0);
        outcome.OverallState.Should().Be(OperationState.Succeeded);
    }

    [Fact]
    public void Computed_WhenSomeResourcesFailed_ReportsFailed()
    {
        // Arrange
        var outcome = new SeedOutcome(
            "op-2",
            "messaging-starter",
            [
                new SeedResourceResult("sqs", "Queue", "q", true, null),
                new SeedResourceResult("sns", "Topic", "t", false, "boom"),
            ]);

        // Assert
        outcome.TotalCount.Should().Be(2);
        outcome.SucceededCount.Should().Be(1);
        outcome.FailedCount.Should().Be(1);
        outcome.OverallState.Should().Be(OperationState.Failed);
    }
}
