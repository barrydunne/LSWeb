using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.StartExecution;
using Foundation.Application.Queries.GetExecutionHistory;
using Foundation.Application.Queries.GetStateMachine;
using Foundation.Application.Queries.ListExecutions;
using Foundation.Application.Queries.ListStateMachines;
using Foundation.Domain.StepFunctions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class StepFunctionsControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<StepFunctionsController> _logger =
        Substitute.For<ILogger<StepFunctionsController>>();

    private StepFunctionsController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListStateMachines_WhenQuerySucceeds_ReturnsOkWithStateMachines()
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
        _sender
            .Send(Arg.Any<ListStateMachinesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListStateMachinesQueryResult>>(
                new ListStateMachinesQueryResult(stateMachines)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStateMachines(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<StateMachineListResponse>>().Subject;
        var stateMachine = ok.Value!.StateMachines.Should().ContainSingle().Subject;
        stateMachine.Name.Should().Be("orders-workflow");
        stateMachine.StateMachineArn.Should()
            .Be("arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow");
        stateMachine.Type.Should().Be("STANDARD");
        stateMachine.CreationDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task ListStateMachines_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListStateMachinesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListStateMachinesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStateMachines(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetStateMachine_WhenQuerySucceeds_ReturnsOkWithDetailsAndForwardsArn()
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
        GetStateMachineQuery? captured = null;
        _sender
            .Send(Arg.Do<GetStateMachineQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetStateMachineQueryResult>>(
                new GetStateMachineQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetStateMachine(arn, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<StateMachineDetailResponse>>().Subject;
        ok.Value!.Name.Should().Be("orders-workflow");
        ok.Value.StateMachineArn.Should().Be(arn);
        ok.Value.Type.Should().Be("STANDARD");
        ok.Value.Status.Should().Be("ACTIVE");
        ok.Value.RoleArn.Should().Be("arn:aws:iam::000000000000:role/service-role/states");
        ok.Value.Definition.Should().Be("{\"StartAt\":\"Done\"}");
        ok.Value.CreationDate.Should().Be(DateTimeOffset.UnixEpoch);
        captured.Should().NotBeNull();
        captured!.StateMachineArn.Should().Be(arn);
    }

    [Fact]
    public async Task GetStateMachine_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetStateMachineQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetStateMachineQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetStateMachine(
            "arn:aws:states:eu-west-1:000000000000:stateMachine:missing",
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListExecutions_WhenQuerySucceeds_ReturnsOkWithExecutionsAndForwardsArn()
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
        ListExecutionsQuery? captured = null;
        _sender
            .Send(Arg.Do<ListExecutionsQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListExecutionsQueryResult>>(
                new ListExecutionsQueryResult(executions)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListExecutions(arn, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ExecutionListResponse>>().Subject;
        var execution = ok.Value!.Executions.Should().ContainSingle().Subject;
        execution.ExecutionArn.Should()
            .Be("arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1");
        execution.Name.Should().Be("run-1");
        execution.StateMachineArn.Should().Be(arn);
        execution.Status.Should().Be("SUCCEEDED");
        execution.StartDate.Should().Be(DateTimeOffset.UnixEpoch);
        execution.StopDate.Should().Be(DateTimeOffset.UnixEpoch.AddMinutes(1));
        captured.Should().NotBeNull();
        captured!.StateMachineArn.Should().Be(arn);
    }

    [Fact]
    public async Task ListExecutions_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListExecutionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListExecutionsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListExecutions(
            "arn:aws:states:eu-west-1:000000000000:stateMachine:missing",
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task StartExecution_WhenCommandSucceeds_ReturnsOkWithResultAndForwardsRequest()
    {
        // Arrange
        const string arn = "arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow";
        var startResult = new ExecutionStartResult(
            "arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1",
            DateTimeOffset.UnixEpoch);
        StartExecutionCommand? captured = null;
        _sender
            .Send(Arg.Do<StartExecutionCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ExecutionStartResult>>(startResult));
        var sut = CreateSut();

        // Act
        var result = await sut.StartExecution(
            new StartExecutionRequest(arn, "run-1", "{\"key\":\"value\"}"),
            TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<StartExecutionResponse>>().Subject;
        ok.Value!.ExecutionArn.Should()
            .Be("arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1");
        ok.Value.StartDate.Should().Be(DateTimeOffset.UnixEpoch);
        captured.Should().NotBeNull();
        captured!.StateMachineArn.Should().Be(arn);
        captured.Name.Should().Be("run-1");
        captured.Input.Should().Be("{\"key\":\"value\"}");
    }

    [Fact]
    public async Task StartExecution_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<StartExecutionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ExecutionStartResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.StartExecution(
            new StartExecutionRequest(
                "arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow", null, null),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetExecutionHistory_WhenQuerySucceeds_ReturnsOkWithEventsAndForwardsArn()
    {
        // Arrange
        const string arn = "arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1";
        IReadOnlyList<ExecutionHistoryEvent> events =
        [
            new(
                2,
                1,
                "TaskStateExited",
                DateTimeOffset.UnixEpoch,
                "DoWork",
                "{\"in\":1}",
                "{\"out\":2}",
                "States.Timeout",
                "It timed out"),
        ];
        GetExecutionHistoryQuery? captured = null;
        _sender
            .Send(Arg.Do<GetExecutionHistoryQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetExecutionHistoryQueryResult>>(
                new GetExecutionHistoryQueryResult(events)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetExecutionHistory(arn, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ExecutionHistoryResponse>>().Subject;
        var historyEvent = ok.Value!.Events.Should().ContainSingle().Subject;
        historyEvent.Id.Should().Be(2);
        historyEvent.PreviousEventId.Should().Be(1);
        historyEvent.Type.Should().Be("TaskStateExited");
        historyEvent.Timestamp.Should().Be(DateTimeOffset.UnixEpoch);
        historyEvent.Name.Should().Be("DoWork");
        historyEvent.Input.Should().Be("{\"in\":1}");
        historyEvent.Output.Should().Be("{\"out\":2}");
        historyEvent.Error.Should().Be("States.Timeout");
        historyEvent.Cause.Should().Be("It timed out");
        captured.Should().NotBeNull();
        captured!.ExecutionArn.Should().Be(arn);
    }

    [Fact]
    public async Task GetExecutionHistory_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetExecutionHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetExecutionHistoryQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetExecutionHistory(
            "arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:missing",
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
