using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListStateMachines;
using Foundation.Application.StepFunctions;
using Foundation.Domain.StepFunctions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListStateMachines;

public class ListStateMachinesQueryHandlerTests
{
    private readonly IStepFunctionsClient _client = Substitute.For<IStepFunctionsClient>();

    private ListStateMachinesQueryHandler CreateSut()
        => new(_client, NullLogger<ListStateMachinesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsStateMachines()
    {
        // Arrange
        IReadOnlyList<StateMachineSummary> stateMachines =
        [
            new(
                "orders-workflow",
                "arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow",
                "STANDARD",
                DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListStateMachinesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stateMachines)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListStateMachinesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StateMachines.Should().ContainSingle(_ => _.Name == "orders-workflow");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListStateMachinesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<StateMachineSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListStateMachinesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
