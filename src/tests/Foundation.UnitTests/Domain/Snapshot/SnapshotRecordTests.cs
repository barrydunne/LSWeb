using Foundation.Domain.Snapshot;

namespace Foundation.UnitTests.Domain.Snapshot;

public class SnapshotRecordTests
{
    [Fact]
    public void SnapshotResourceData_ExposesAllProperties()
    {
        var data = new SnapshotResourceData("lambda", "Function", "func-1", "test-func", "{}");

        data.ServiceKey.Should().Be("lambda");
        data.ResourceType.Should().Be("Function");
        data.ResourceId.Should().Be("func-1");
        data.ResourceName.Should().Be("test-func");
        data.Data.Should().Be("{}");
    }

    [Fact]
    public void SnapshotResourceData_SupportsValueEquality()
    {
        var data = new SnapshotResourceData("lambda", "Function", "func-1", "test-func", "{}");
        var same = data with { };

        data.Should().Be(same);
        data.GetHashCode().Should().Be(same.GetHashCode());
        data.ToString().Should().Contain("Function");
        (data with { ServiceKey = "sqs" }).Should().NotBe(data);
        (data with { ResourceType = "Queue" }).Should().NotBe(data);
        (data with { ResourceId = "other" }).Should().NotBe(data);
        (data with { ResourceName = "other" }).Should().NotBe(data);
        (data with { Data = "{\"k\":1}" }).Should().NotBe(data);
    }

    [Fact]
    public void SnapshotFailureDetail_ExposesAllProperties()
    {
        var failure = new SnapshotFailureDetail("lambda", "func-1", "boom");

        failure.Service.Should().Be("lambda");
        failure.ResourceId.Should().Be("func-1");
        failure.Error.Should().Be("boom");
    }

    [Fact]
    public void SnapshotFailureDetail_SupportsValueEquality()
    {
        var failure = new SnapshotFailureDetail("lambda", "func-1", "boom");
        var same = failure with { };

        failure.Should().Be(same);
        failure.GetHashCode().Should().Be(same.GetHashCode());
        failure.ToString().Should().Contain("boom");
        (failure with { Service = "sqs" }).Should().NotBe(failure);
        (failure with { ResourceId = "other" }).Should().NotBe(failure);
        (failure with { Error = "other" }).Should().NotBe(failure);
    }
}
