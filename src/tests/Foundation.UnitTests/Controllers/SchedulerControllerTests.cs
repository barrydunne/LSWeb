using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateSchedule;
using Foundation.Application.Commands.CreateScheduleGroup;
using Foundation.Application.Commands.DeleteSchedule;
using Foundation.Application.Commands.DeleteScheduleGroup;
using Foundation.Application.Commands.UpdateSchedule;
using Foundation.Application.Queries.GetSchedule;
using Foundation.Application.Queries.ListScheduleGroups;
using Foundation.Application.Queries.ListSchedules;
using Foundation.Domain.Scheduler;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SchedulerControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SchedulerController> _logger =
        Substitute.For<ILogger<SchedulerController>>();

    private SchedulerController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListSchedules_WhenQuerySucceeds_ReturnsOkWithSchedules()
    {
        // Arrange
        IReadOnlyList<ScheduleSummary> schedules =
        [
            new(
                "nightly",
                "default",
                "ENABLED",
                "arn:aws:lambda:eu-west-1:000000000000:function:run",
                "arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly"),
        ];
        _sender
            .Send(Arg.Any<ListSchedulesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSchedulesQueryResult>>(
                new ListSchedulesQueryResult(schedules)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSchedules(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ScheduleListResponse>>().Subject;
        var schedule = ok.Value!.Schedules.Should().ContainSingle().Subject;
        schedule.Name.Should().Be("nightly");
        schedule.GroupName.Should().Be("default");
        schedule.State.Should().Be("ENABLED");
        schedule.TargetArn.Should().Be("arn:aws:lambda:eu-west-1:000000000000:function:run");
        schedule.Arn.Should().Be("arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly");
    }

    [Fact]
    public async Task ListSchedules_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSchedulesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSchedulesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSchedules(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetSchedule_WhenQuerySucceeds_ReturnsOkWithDetailsAndForwardsIdentity()
    {
        // Arrange
        var detail = new ScheduleDetail(
            "nightly",
            "default",
            "ENABLED",
            "rate(1 day)",
            "UTC",
            "Nightly run",
            DateTimeOffset.UnixEpoch,
            null,
            "arn:aws:lambda:eu-west-1:000000000000:function:run",
            "arn:aws:iam::000000000000:role/scheduler",
            "OFF",
            null,
            "arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly",
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);
        GetScheduleQuery? captured = null;
        _sender
            .Send(Arg.Do<GetScheduleQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetScheduleQueryResult>>(
                new GetScheduleQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetSchedule("nightly", "default", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ScheduleDetailResponse>>().Subject;
        ok.Value!.Name.Should().Be("nightly");
        ok.Value.GroupName.Should().Be("default");
        ok.Value.State.Should().Be("ENABLED");
        ok.Value.ScheduleExpression.Should().Be("rate(1 day)");
        ok.Value.ScheduleExpressionTimezone.Should().Be("UTC");
        ok.Value.Description.Should().Be("Nightly run");
        ok.Value.StartDate.Should().Be(DateTimeOffset.UnixEpoch);
        ok.Value.EndDate.Should().BeNull();
        ok.Value.TargetArn.Should().Be("arn:aws:lambda:eu-west-1:000000000000:function:run");
        ok.Value.RoleArn.Should().Be("arn:aws:iam::000000000000:role/scheduler");
        ok.Value.FlexibleTimeWindowMode.Should().Be("OFF");
        ok.Value.MaximumWindowInMinutes.Should().BeNull();
        ok.Value.Arn.Should().Be("arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly");
        ok.Value.CreationDate.Should().Be(DateTimeOffset.UnixEpoch);
        ok.Value.LastModificationDate.Should().Be(DateTimeOffset.UnixEpoch);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("nightly");
        captured.GroupName.Should().Be("default");
    }

    [Fact]
    public async Task GetSchedule_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetScheduleQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetScheduleQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetSchedule("missing", "default", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateSchedule_WhenCommandSucceeds_ReturnsCreatedAndForwardsFields()
    {
        // Arrange
        var request = new ScheduleCreateRequest(
            "nightly",
            "default",
            "rate(5 minutes)",
            "UTC",
            "nightly run",
            null,
            null,
            "arn:aws:sqs:us-east-1:000000000000:queue",
            "arn:aws:iam::000000000000:role/scheduler",
            "OFF",
            null,
            "ENABLED");
        CreateScheduleCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateScheduleCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateSchedule(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("nightly");
        captured.GroupName.Should().Be("default");
        captured.ScheduleExpression.Should().Be("rate(5 minutes)");
        captured.State.Should().Be("ENABLED");
    }

    [Fact]
    public async Task CreateSchedule_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new ScheduleCreateRequest(
            "nightly",
            "default",
            "rate(5 minutes)",
            null,
            null,
            null,
            null,
            "arn:aws:sqs:us-east-1:000000000000:queue",
            "arn:aws:iam::000000000000:role/scheduler",
            "OFF",
            null,
            "ENABLED");
        _sender
            .Send(Arg.Any<CreateScheduleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateSchedule(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateSchedule_WhenCommandSucceeds_ReturnsNoContentAndForwardsFields()
    {
        // Arrange
        var request = new ScheduleUpdateRequest(
            "rate(10 minutes)",
            "UTC",
            "nightly run",
            null,
            null,
            "arn:aws:sqs:us-east-1:000000000000:queue",
            "arn:aws:iam::000000000000:role/scheduler",
            "FLEXIBLE",
            15,
            "DISABLED");
        UpdateScheduleCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateScheduleCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateSchedule("nightly", "default", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("nightly");
        captured.GroupName.Should().Be("default");
        captured.ScheduleExpression.Should().Be("rate(10 minutes)");
        captured.FlexibleTimeWindowMode.Should().Be("FLEXIBLE");
        captured.MaximumWindowInMinutes.Should().Be(15);
        captured.State.Should().Be("DISABLED");
    }

    [Fact]
    public async Task UpdateSchedule_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new ScheduleUpdateRequest(
            "rate(10 minutes)",
            null,
            null,
            null,
            null,
            "arn:aws:sqs:us-east-1:000000000000:queue",
            "arn:aws:iam::000000000000:role/scheduler",
            "OFF",
            null,
            "ENABLED");
        _sender
            .Send(Arg.Any<UpdateScheduleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateSchedule("nightly", "default", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteSchedule_WhenCommandSucceeds_ReturnsNoContentAndForwardsIdentity()
    {
        // Arrange
        DeleteScheduleCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteScheduleCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteSchedule("nightly", "default", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("nightly");
        captured.GroupName.Should().Be("default");
    }

    [Fact]
    public async Task DeleteSchedule_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteScheduleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteSchedule("nightly", "default", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListScheduleGroups_WhenQuerySucceeds_ReturnsOkWithGroups()
    {
        // Arrange
        IReadOnlyList<ScheduleGroup> groups =
        [
            new(
                "default",
                "ACTIVE",
                "arn:aws:scheduler:eu-west-1:000000000000:schedule-group/default",
                DateTimeOffset.UnixEpoch,
                DateTimeOffset.UnixEpoch),
        ];
        _sender
            .Send(Arg.Any<ListScheduleGroupsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListScheduleGroupsQueryResult>>(
                new ListScheduleGroupsQueryResult(groups)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListScheduleGroups(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ScheduleGroupListResponse>>().Subject;
        var group = ok.Value!.Groups.Should().ContainSingle().Subject;
        group.Name.Should().Be("default");
        group.State.Should().Be("ACTIVE");
        group.Arn.Should().Be("arn:aws:scheduler:eu-west-1:000000000000:schedule-group/default");
        group.CreationDate.Should().Be(DateTimeOffset.UnixEpoch);
        group.LastModificationDate.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public async Task ListScheduleGroups_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListScheduleGroupsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListScheduleGroupsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListScheduleGroups(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateScheduleGroup_WhenCommandSucceeds_ReturnsCreatedAndForwardsName()
    {
        // Arrange
        CreateScheduleGroupCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateScheduleGroupCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateScheduleGroup(
            new ScheduleGroupCreateRequest("reports"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("reports");
    }

    [Fact]
    public async Task CreateScheduleGroup_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateScheduleGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateScheduleGroup(
            new ScheduleGroupCreateRequest("reports"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteScheduleGroup_WhenCommandSucceeds_ReturnsNoContentAndForwardsName()
    {
        // Arrange
        DeleteScheduleGroupCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteScheduleGroupCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteScheduleGroup("reports", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("reports");
    }

    [Fact]
    public async Task DeleteScheduleGroup_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteScheduleGroupCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteScheduleGroup("reports", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
