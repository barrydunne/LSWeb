using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateSchedule;
using Foundation.Application.Commands.CreateScheduleGroup;
using Foundation.Application.Commands.DeleteSchedule;
using Foundation.Application.Commands.DeleteScheduleGroup;
using Foundation.Application.Commands.UpdateSchedule;
using Foundation.Application.Queries.GetSchedule;
using Foundation.Application.Queries.ListScheduleGroups;
using Foundation.Application.Queries.ListSchedules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to EventBridge Scheduler: listing the available schedules and viewing the
/// details of a single schedule.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/scheduler")]
public partial class SchedulerController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public SchedulerController(ISender sender, ILogger<SchedulerController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the EventBridge Scheduler schedules available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the schedule summaries.</returns>
    [HttpGet("schedules")]
    [ProducesResponseType(typeof(ScheduleListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListSchedules(CancellationToken cancellationToken)
    {
        LogHandlingListSchedules();
        var result = await _sender.Send(new ListSchedulesQuery(), cancellationToken);
        LogListSchedulesHandled(result.IsSuccess);
        return result.Match(
            schedules => Results.Ok(new ScheduleListResponse(
                schedules.Schedules
                    .Select(schedule => new ScheduleSummaryResponse(
                        schedule.Name,
                        schedule.GroupName,
                        schedule.State,
                        schedule.TargetArn,
                        schedule.Arn))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single EventBridge Scheduler schedule by its name and group.
    /// </summary>
    /// <param name="name">The name of the schedule to read.</param>
    /// <param name="group">The name of the schedule group the schedule belongs to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the schedule details.</returns>
    [HttpGet("schedule")]
    [ProducesResponseType(typeof(ScheduleDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetSchedule(
        [FromQuery] string name, [FromQuery] string group, CancellationToken cancellationToken)
    {
        LogHandlingGetSchedule(group, name);
        var result = await _sender.Send(new GetScheduleQuery(name, group), cancellationToken);
        LogGetScheduleHandled(result.IsSuccess);
        return result.Match(
            schedule => Results.Ok(new ScheduleDetailResponse(
                schedule.Schedule.Name,
                schedule.Schedule.GroupName,
                schedule.Schedule.State,
                schedule.Schedule.ScheduleExpression,
                schedule.Schedule.ScheduleExpressionTimezone,
                schedule.Schedule.Description,
                schedule.Schedule.StartDate,
                schedule.Schedule.EndDate,
                schedule.Schedule.TargetArn,
                schedule.Schedule.RoleArn,
                schedule.Schedule.FlexibleTimeWindowMode,
                schedule.Schedule.MaximumWindowInMinutes,
                schedule.Schedule.Arn,
                schedule.Schedule.CreationDate,
                schedule.Schedule.LastModificationDate)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new EventBridge Scheduler schedule.
    /// </summary>
    /// <param name="request">The schedule configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created schedule.</returns>
    [HttpPost("schedules")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateSchedule(
        [FromBody] ScheduleCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateSchedule(request.GroupName, request.Name);
        var result = await _sender.Send(
            new CreateScheduleCommand(
                request.Name,
                request.GroupName,
                request.ScheduleExpression,
                request.ScheduleExpressionTimezone,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.TargetArn,
                request.RoleArn,
                request.FlexibleTimeWindowMode,
                request.MaximumWindowInMinutes,
                request.State),
            cancellationToken);
        LogCreateScheduleHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/scheduler/schedules/{Uri.EscapeDataString(request.Name)}?group={Uri.EscapeDataString(request.GroupName)}",
                null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates an existing EventBridge Scheduler schedule.
    /// </summary>
    /// <param name="name">The name of the schedule to update.</param>
    /// <param name="group">The name of the schedule group the schedule belongs to.</param>
    /// <param name="request">The new schedule configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("schedules/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateSchedule(
        string name, [FromQuery] string group, [FromBody] ScheduleUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateSchedule(group, name);
        var result = await _sender.Send(
            new UpdateScheduleCommand(
                name,
                group,
                request.ScheduleExpression,
                request.ScheduleExpressionTimezone,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.TargetArn,
                request.RoleArn,
                request.FlexibleTimeWindowMode,
                request.MaximumWindowInMinutes,
                request.State),
            cancellationToken);
        LogUpdateScheduleHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an EventBridge Scheduler schedule by its name and group. This action cannot be undone.
    /// </summary>
    /// <param name="name">The name of the schedule to delete.</param>
    /// <param name="group">The name of the schedule group the schedule belongs to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("schedules/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteSchedule(
        string name, [FromQuery] string group, CancellationToken cancellationToken)
    {
        LogHandlingDeleteSchedule(group, name);
        var result = await _sender.Send(new DeleteScheduleCommand(name, group), cancellationToken);
        LogDeleteScheduleHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the EventBridge Scheduler schedule groups available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the schedule groups.</returns>
    [HttpGet("groups")]
    [ProducesResponseType(typeof(ScheduleGroupListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListScheduleGroups(CancellationToken cancellationToken)
    {
        LogHandlingListScheduleGroups();
        var result = await _sender.Send(new ListScheduleGroupsQuery(), cancellationToken);
        LogListScheduleGroupsHandled(result.IsSuccess);
        return result.Match(
            groups => Results.Ok(new ScheduleGroupListResponse(
                groups.Groups
                    .Select(group => new ScheduleGroupResponse(
                        group.Name,
                        group.State,
                        group.Arn,
                        group.CreationDate,
                        group.LastModificationDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new EventBridge Scheduler schedule group.
    /// </summary>
    /// <param name="request">The schedule group configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created schedule group.</returns>
    [HttpPost("groups")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateScheduleGroup(
        [FromBody] ScheduleGroupCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateScheduleGroup(request.Name);
        var result = await _sender.Send(new CreateScheduleGroupCommand(request.Name), cancellationToken);
        LogCreateScheduleGroupHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/scheduler/groups/{Uri.EscapeDataString(request.Name)}",
                null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an EventBridge Scheduler schedule group by its name. The <c>default</c> group cannot
    /// be deleted. This action cannot be undone.
    /// </summary>
    /// <param name="name">The name of the schedule group to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("groups/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteScheduleGroup(string name, CancellationToken cancellationToken)
    {
        LogHandlingDeleteScheduleGroup(name);
        var result = await _sender.Send(new DeleteScheduleGroupCommand(name), cancellationToken);
        LogDeleteScheduleGroupHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling Scheduler schedule list request.")]
    private partial void LogHandlingListSchedules();

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule list request handled. Success: {Success}")]
    private partial void LogListSchedulesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Scheduler schedule get request for {Group}/{Name}.")]
    private partial void LogHandlingGetSchedule(string group, string name);

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule get request handled. Success: {Success}")]
    private partial void LogGetScheduleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Scheduler schedule create request for {Group}/{Name}.")]
    private partial void LogHandlingCreateSchedule(string group, string name);

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule create request handled. Success: {Success}")]
    private partial void LogCreateScheduleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Scheduler schedule update request for {Group}/{Name}.")]
    private partial void LogHandlingUpdateSchedule(string group, string name);

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule update request handled. Success: {Success}")]
    private partial void LogUpdateScheduleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Scheduler schedule delete request for {Group}/{Name}.")]
    private partial void LogHandlingDeleteSchedule(string group, string name);

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule delete request handled. Success: {Success}")]
    private partial void LogDeleteScheduleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Scheduler schedule group list request.")]
    private partial void LogHandlingListScheduleGroups();

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule group list request handled. Success: {Success}")]
    private partial void LogListScheduleGroupsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Scheduler schedule group create request for {Name}.")]
    private partial void LogHandlingCreateScheduleGroup(string name);

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule group create request handled. Success: {Success}")]
    private partial void LogCreateScheduleGroupHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Scheduler schedule group delete request for {Name}.")]
    private partial void LogHandlingDeleteScheduleGroup(string name);

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule group delete request handled. Success: {Success}")]
    private partial void LogDeleteScheduleGroupHandled(bool success);
}
