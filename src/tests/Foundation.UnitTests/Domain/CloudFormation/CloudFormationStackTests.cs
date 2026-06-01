using Foundation.Domain.CloudFormation;

namespace Foundation.UnitTests.Domain.CloudFormation;

public class CloudFormationStackTests
{
    [Fact]
    public void StackSummary_ExposesAllProperties()
    {
        var created = DateTimeOffset.UnixEpoch;
        var updated = created.AddHours(1);
        var summary = new CloudFormationStackSummary(
            "orders-stack",
            "arn:stack",
            "CREATE_COMPLETE",
            "An orders stack",
            created,
            updated);

        summary.StackName.Should().Be("orders-stack");
        summary.StackId.Should().Be("arn:stack");
        summary.StackStatus.Should().Be("CREATE_COMPLETE");
        summary.Description.Should().Be("An orders stack");
        summary.CreationTime.Should().Be(created);
        summary.LastUpdatedTime.Should().Be(updated);
    }

    [Fact]
    public void StackSummary_Equality_DistinguishesByValue()
    {
        var created = DateTimeOffset.UnixEpoch;
        var first = new CloudFormationStackSummary("a", "id", "S", null, created, null);
        var same = new CloudFormationStackSummary("a", "id", "S", null, created, null);
        var different = new CloudFormationStackSummary("b", "id", "S", null, created, null);

        first.Should().Be(same);
        first.Should().NotBe(different);
    }

    [Fact]
    public void StackDetail_ExposesAllProperties()
    {
        var created = DateTimeOffset.UnixEpoch;
        var updated = created.AddHours(2);
        IReadOnlyList<StackParameter> parameters = [new("Env", "dev")];
        IReadOnlyList<StackOutput> outputs = [new("Url", "https://x", "The url", " export")];
        IReadOnlyList<StackTag> tags = [new("team", "orders")];
        IReadOnlyList<string> capabilities = ["CAPABILITY_IAM"];
        var detail = new CloudFormationStackDetail(
            "orders-stack",
            "arn:stack",
            "UPDATE_COMPLETE",
            "User initiated",
            "An orders stack",
            created,
            updated,
            parameters,
            outputs,
            tags,
            capabilities);

        detail.StackName.Should().Be("orders-stack");
        detail.StackId.Should().Be("arn:stack");
        detail.StackStatus.Should().Be("UPDATE_COMPLETE");
        detail.StackStatusReason.Should().Be("User initiated");
        detail.Description.Should().Be("An orders stack");
        detail.CreationTime.Should().Be(created);
        detail.LastUpdatedTime.Should().Be(updated);
        detail.Parameters.Should().BeSameAs(parameters);
        detail.Outputs.Should().BeSameAs(outputs);
        detail.Tags.Should().BeSameAs(tags);
        detail.Capabilities.Should().BeSameAs(capabilities);
    }

    [Fact]
    public void StackDetail_Equality_DistinguishesByValue()
    {
        var created = DateTimeOffset.UnixEpoch;
        var first = new CloudFormationStackDetail(
            "a", "id", "S", null, null, created, null, [], [], [], []);
        var same = new CloudFormationStackDetail(
            "a", "id", "S", null, null, created, null, [], [], [], []);
        var different = first with { StackName = "b" };

        first.Should().Be(same);
        first.Should().NotBe(different);
    }

    [Fact]
    public void StackParameter_ExposesValuesAndEquality()
    {
        var parameter = new StackParameter("Env", "dev");

        parameter.ParameterKey.Should().Be("Env");
        parameter.ParameterValue.Should().Be("dev");
        parameter.Should().Be(new StackParameter("Env", "dev"));
        parameter.Should().NotBe(new StackParameter("Env", "prod"));
    }

    [Fact]
    public void StackOutput_ExposesValuesAndEquality()
    {
        var output = new StackOutput("Url", "https://x", "The url", "the-export");

        output.OutputKey.Should().Be("Url");
        output.OutputValue.Should().Be("https://x");
        output.Description.Should().Be("The url");
        output.ExportName.Should().Be("the-export");
        output.Should().Be(new StackOutput("Url", "https://x", "The url", "the-export"));
        output.Should().NotBe(new StackOutput("Url", "https://y", null, null));
    }

    [Fact]
    public void StackTag_ExposesValuesAndEquality()
    {
        var tag = new StackTag("team", "orders");

        tag.Key.Should().Be("team");
        tag.Value.Should().Be("orders");
        tag.Should().Be(new StackTag("team", "orders"));
        tag.Should().NotBe(new StackTag("team", "billing"));
    }

    [Fact]
    public void StackTemplate_ExposesValuesAndEquality()
    {
        var template = new CloudFormationStackTemplate("{\"Resources\":{}}", "json");

        template.TemplateBody.Should().Be("{\"Resources\":{}}");
        template.Format.Should().Be("json");
        template.Should().Be(new CloudFormationStackTemplate("{\"Resources\":{}}", "json"));
        template.Should().NotBe(new CloudFormationStackTemplate("Resources: {}", "yaml"));
    }

    [Fact]
    public void StackResource_ExposesValuesAndEquality()
    {
        var timestamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var resource = new StackResource(
            "OrdersQueue",
            "orders-queue",
            "AWS::SQS::Queue",
            "CREATE_COMPLETE",
            "Resource creation initiated",
            timestamp);

        resource.LogicalResourceId.Should().Be("OrdersQueue");
        resource.PhysicalResourceId.Should().Be("orders-queue");
        resource.ResourceType.Should().Be("AWS::SQS::Queue");
        resource.ResourceStatus.Should().Be("CREATE_COMPLETE");
        resource.ResourceStatusReason.Should().Be("Resource creation initiated");
        resource.LastUpdatedTime.Should().Be(timestamp);
        resource.Should().Be(new StackResource(
            "OrdersQueue",
            "orders-queue",
            "AWS::SQS::Queue",
            "CREATE_COMPLETE",
            "Resource creation initiated",
            timestamp));
        resource.Should().NotBe(new StackResource(
            "OrdersQueue",
            null,
            "AWS::SQS::Queue",
            "DELETE_COMPLETE",
            null,
            timestamp));
    }

    [Fact]
    public void StackEvent_ExposesValuesAndEquality()
    {
        var timestamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var stackEvent = new StackEvent(
            "event-1",
            timestamp,
            "OrdersQueue",
            "orders-queue",
            "AWS::SQS::Queue",
            "CREATE_COMPLETE",
            "Resource creation initiated");

        stackEvent.EventId.Should().Be("event-1");
        stackEvent.Timestamp.Should().Be(timestamp);
        stackEvent.LogicalResourceId.Should().Be("OrdersQueue");
        stackEvent.PhysicalResourceId.Should().Be("orders-queue");
        stackEvent.ResourceType.Should().Be("AWS::SQS::Queue");
        stackEvent.ResourceStatus.Should().Be("CREATE_COMPLETE");
        stackEvent.ResourceStatusReason.Should().Be("Resource creation initiated");
        stackEvent.Should().Be(new StackEvent(
            "event-1",
            timestamp,
            "OrdersQueue",
            "orders-queue",
            "AWS::SQS::Queue",
            "CREATE_COMPLETE",
            "Resource creation initiated"));
        stackEvent.Should().NotBe(new StackEvent(
            "event-2",
            timestamp,
            "OrdersQueue",
            null,
            "AWS::SQS::Queue",
            "DELETE_COMPLETE",
            null));
    }
}
