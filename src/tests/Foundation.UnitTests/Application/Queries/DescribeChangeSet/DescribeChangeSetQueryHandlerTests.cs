using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.DescribeChangeSet;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.DescribeChangeSet;

public class DescribeChangeSetQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private DescribeChangeSetQueryHandler CreateSut()
        => new(_client, NullLogger<DescribeChangeSetQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    private static ChangeSetDetail BuildDetail()
        => new(
            "add-queue",
            "arn:changeset/add-queue",
            "orders-stack",
            "arn:stack/orders-stack",
            "CREATE_COMPLETE",
            null,
            "AVAILABLE",
            "Adds a queue",
            DateTime.UtcNow,
            [new StackParameter("Env", "dev")],
            ["CAPABILITY_IAM"],
            [new ResourceChange("Add", "OrdersQueue", null, "AWS::SQS::Queue", null)]);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsChangeSetDetail()
    {
        // Arrange
        var detail = BuildDetail();
        var success = Ok(detail);
        _client
            .DescribeChangeSetAsync("orders-stack", "add-queue", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(success));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DescribeChangeSetQuery("orders-stack", "add-queue"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ChangeSet.Should().BeSameAs(detail);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .DescribeChangeSetAsync("orders-stack", "add-queue", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ChangeSetDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DescribeChangeSetQuery("orders-stack", "add-queue"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
