using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListExecutions;
using Foundation.Application.StepFunctions;
using Foundation.Domain.StepFunctions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListExecutions;

public class ListExecutionsQueryHandlerTests
{
    private readonly IStepFunctionsClient _client = Substitute.For<IStepFunctionsClient>();

    private ListExecutionsQueryHandler CreateSut()
        => new(_client, NullLogger<ListExecutionsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsExecutionsAndForwardsArn()
    {
        // Arrange
        const string arn = "arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow";
        IReadOnlyList<ExecutionSummary> executions =
        [
            new(
                "arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1",
                "run-1",
                arn,
                "SUCCEEDED",
                DateTimeOffset.UnixEpoch,
                DateTimeOffset.UnixEpoch.AddMinutes(1)),
        ];
        string? capturedArn = null;
        _client
            .ListExecutionsAsync(Arg.Do<string>(value => capturedArn = value), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(executions)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListExecutionsQuery(arn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Executions.Should().ContainSingle(_ => _.Name == "run-1");
        capturedArn.Should().Be(arn);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListExecutionsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<ExecutionSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListExecutionsQuery("arn"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
