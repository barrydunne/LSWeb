using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Application.Queries.ListStackEvents;
using Foundation.Domain.CloudFormation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListStackEvents;

public class ListStackEventsQueryHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();

    private ListStackEventsQueryHandler CreateSut()
        => new(_client, NullLogger<ListStackEventsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsEvents()
    {
        // Arrange
        const string stackName = "orders-stack";
        IReadOnlyList<StackEvent> events =
        [
            new(
                "event-1",
                DateTime.UtcNow,
                "OrdersQueue",
                "orders-queue",
                "AWS::SQS::Queue",
                "CREATE_COMPLETE",
                null),
        ];
        _client
            .DescribeStackEventsAsync(stackName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(events)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListStackEventsQuery(stackName), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Events.Should().BeSameAs(events);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        const string stackName = "orders-stack";
        _client
            .DescribeStackEventsAsync(stackName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<StackEvent>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListStackEventsQuery(stackName), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
