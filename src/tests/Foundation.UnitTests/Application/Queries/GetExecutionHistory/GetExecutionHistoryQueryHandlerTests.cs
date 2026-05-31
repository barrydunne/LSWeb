using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetExecutionHistory;
using Foundation.Application.StepFunctions;
using Foundation.Domain.StepFunctions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetExecutionHistory;

public class GetExecutionHistoryQueryHandlerTests
{
    private readonly IStepFunctionsClient _client = Substitute.For<IStepFunctionsClient>();

    private GetExecutionHistoryQueryHandler CreateSut()
        => new(_client, NullLogger<GetExecutionHistoryQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsEventsAndForwardsArn()
    {
        // Arrange
        const string arn = "arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1";
        IReadOnlyList<ExecutionHistoryEvent> events =
        [
            new(
                1,
                null,
                "ExecutionStarted",
                DateTimeOffset.UnixEpoch,
                null,
                "{\"key\":\"value\"}",
                null,
                null,
                null),
        ];
        string? capturedArn = null;
        _client
            .GetExecutionHistoryAsync(Arg.Do<string>(value => capturedArn = value), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(events)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetExecutionHistoryQuery(arn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Events.Should().ContainSingle(_ => _.Type == "ExecutionStarted");
        capturedArn.Should().Be(arn);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetExecutionHistoryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<ExecutionHistoryEvent>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetExecutionHistoryQuery("arn"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
