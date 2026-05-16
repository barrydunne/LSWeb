using Foundation.Domain.Bulk;
using Foundation.Domain.Streaming;

namespace Foundation.UnitTests.Domain.Bulk;

public class BulkActionOutcomeTests
{
    [Fact]
    public void Counts_WhenAllItemsSucceed_ReportSucceeded()
    {
        var outcome = new BulkActionOutcome("op-1", "delete",
        [
            new BulkActionItemResult("a", true, null),
            new BulkActionItemResult("b", true, null),
        ]);

        outcome.TotalCount.Should().Be(2);
        outcome.SucceededCount.Should().Be(2);
        outcome.FailedCount.Should().Be(0);
        outcome.OverallState.Should().Be(OperationState.Succeeded);
    }

    [Fact]
    public void Counts_WhenSomeItemsFail_ReportPartialFailure()
    {
        var outcome = new BulkActionOutcome("op-1", "delete",
        [
            new BulkActionItemResult("a", true, null),
            new BulkActionItemResult("b", false, "Resource id is required."),
        ]);

        outcome.TotalCount.Should().Be(2);
        outcome.SucceededCount.Should().Be(1);
        outcome.FailedCount.Should().Be(1);
        outcome.OverallState.Should().Be(OperationState.Failed);
    }
}
