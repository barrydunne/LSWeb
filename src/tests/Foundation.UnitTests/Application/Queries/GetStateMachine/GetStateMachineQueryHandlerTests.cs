using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetStateMachine;
using Foundation.Application.StepFunctions;
using Foundation.Domain.StepFunctions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetStateMachine;

public class GetStateMachineQueryHandlerTests
{
    private readonly IStepFunctionsClient _client = Substitute.For<IStepFunctionsClient>();

    private GetStateMachineQueryHandler CreateSut()
        => new(_client, NullLogger<GetStateMachineQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsStateMachine()
    {
        // Arrange
        const string arn = "arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow";
        var detail = new StateMachineDetail(
            "orders-workflow",
            arn,
            "STANDARD",
            "ACTIVE",
            "arn:aws:iam::000000000000:role/service-role/states",
            "{\"StartAt\":\"Done\"}",
            DateTimeOffset.UnixEpoch);
        _client
            .DescribeStateMachineAsync(arn, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetStateMachineQuery(arn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StateMachine.Should().Be(detail);
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        const string arn = "arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow";
        _client
            .DescribeStateMachineAsync(arn, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<StateMachineDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetStateMachineQuery(arn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
