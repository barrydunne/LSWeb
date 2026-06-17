using Foundation.Api.Models;

namespace Foundation.UnitTests.Models;

public class SnapshotResponseTests
{
    [Fact]
    public void SnapshotImportResponse_SupportsValueEquality()
    {
        var failures = new List<SnapshotFailureResponse>
        {
            new("lambda", "func-1", "boom"),
        };
        var response = new SnapshotImportResponse(
            "imp-1", "Import", DateTime.UnixEpoch, 4, 3, 1, failures);

        response.OperationId.Should().Be("imp-1");
        response.OperationType.Should().Be("Import");
        response.CompletedAt.Should().Be(DateTime.UnixEpoch);
        response.TotalResources.Should().Be(4);
        response.SuccessCount.Should().Be(3);
        response.FailureCount.Should().Be(1);
        response.Failures.Should().BeSameAs(failures);

        var same = response with { };
        response.Should().Be(same);
        response.GetHashCode().Should().Be(same.GetHashCode());
        response.ToString().Should().Contain("imp-1");
        (response with { OperationId = "other" }).Should().NotBe(response);
        (response with { OperationType = "other" }).Should().NotBe(response);
        (response with { CompletedAt = DateTime.UnixEpoch.AddDays(1) }).Should().NotBe(response);
        (response with { TotalResources = 9 }).Should().NotBe(response);
        (response with { SuccessCount = 9 }).Should().NotBe(response);
        (response with { FailureCount = 9 }).Should().NotBe(response);
        (response with { Failures = new List<SnapshotFailureResponse>() }).Should().NotBe(response);
    }

    [Fact]
    public void SnapshotFailureResponse_SupportsValueEquality()
    {
        var failure = new SnapshotFailureResponse("lambda", "func-1", "boom");

        failure.Service.Should().Be("lambda");
        failure.ResourceId.Should().Be("func-1");
        failure.Error.Should().Be("boom");

        var same = failure with { };
        failure.Should().Be(same);
        failure.GetHashCode().Should().Be(same.GetHashCode());
        failure.ToString().Should().Contain("boom");
        (failure with { Service = "sqs" }).Should().NotBe(failure);
        (failure with { ResourceId = "other" }).Should().NotBe(failure);
        (failure with { Error = "other" }).Should().NotBe(failure);
    }
}
