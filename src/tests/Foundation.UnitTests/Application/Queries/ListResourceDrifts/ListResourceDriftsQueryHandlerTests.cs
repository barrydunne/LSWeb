using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.ListResourceDrifts;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListResourceDrifts;

public class ListResourceDriftsQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private ListResourceDriftsQueryHandler CreateSut()
        => new(_client, NullLogger<ListResourceDriftsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    private static StackResourceDrift BuildDrift()
        => new(
            "OrdersQueue",
            "https://sqs/orders",
            "AWS::SQS::Queue",
            "MODIFIED",
            "{\"DelaySeconds\":\"0\"}",
            "{\"DelaySeconds\":\"30\"}",
            DateTimeOffset.UtcNow);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsDrifts()
    {
        // Arrange
        IReadOnlyList<StackResourceDrift> drifts = [BuildDrift()];
        var success = Ok(drifts);
        _client
            .DescribeStackResourceDriftsAsync("orders-stack", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(success));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListResourceDriftsQuery("orders-stack"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Drifts.Should().BeSameAs(drifts);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .DescribeStackResourceDriftsAsync("orders-stack", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<StackResourceDrift>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListResourceDriftsQuery("orders-stack"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
