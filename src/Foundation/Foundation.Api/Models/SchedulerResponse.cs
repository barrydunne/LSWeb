namespace Foundation.Api.Models;

/// <summary>
/// The EventBridge Scheduler schedules available on the backend.
/// </summary>
/// <param name="Schedules">The schedule summaries, ordered as returned by the backend.</param>
public sealed record ScheduleListResponse(
    IReadOnlyList<ScheduleSummaryResponse> Schedules);

/// <summary>
/// A concise view of an EventBridge Scheduler schedule as it appears in a list.
/// </summary>
/// <param name="Name">The schedule name, unique within its group.</param>
/// <param name="GroupName">The name of the schedule group the schedule belongs to.</param>
/// <param name="State">Whether the schedule is enabled or disabled.</param>
/// <param name="TargetArn">The Amazon Resource Name of the target the schedule invokes.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the schedule.</param>
public sealed record ScheduleSummaryResponse(
    string Name,
    string GroupName,
    string State,
    string TargetArn,
    string Arn);

/// <summary>
/// The full configuration of an EventBridge Scheduler schedule.
/// </summary>
/// <param name="Name">The schedule name, unique within its group.</param>
/// <param name="GroupName">The name of the schedule group the schedule belongs to.</param>
/// <param name="State">Whether the schedule is enabled or disabled.</param>
/// <param name="ScheduleExpression">The timing expression that controls when the schedule runs.</param>
/// <param name="ScheduleExpressionTimezone">The timezone in which the schedule expression is evaluated, or <see langword="null"/> when not specified.</param>
/// <param name="Description">A human-readable description of the schedule, or <see langword="null"/> when not specified.</param>
/// <param name="StartDate">The earliest moment the schedule may run, or <see langword="null"/> when not specified.</param>
/// <param name="EndDate">The latest moment the schedule may run, or <see langword="null"/> when not specified.</param>
/// <param name="TargetArn">The Amazon Resource Name of the target the schedule invokes.</param>
/// <param name="RoleArn">The Amazon Resource Name of the role the scheduler assumes to invoke the target.</param>
/// <param name="FlexibleTimeWindowMode">The flexible time window mode, either <c>OFF</c> or <c>FLEXIBLE</c>.</param>
/// <param name="MaximumWindowInMinutes">The maximum flexible time window in minutes, or <see langword="null"/> when the mode is off.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the schedule.</param>
/// <param name="CreationDate">The moment the schedule was created, or <see langword="null"/> when not reported.</param>
/// <param name="LastModificationDate">The moment the schedule was last modified, or <see langword="null"/> when not reported.</param>
/// <param name="TargetInput">A constant JSON payload passed to the target when the schedule runs, or <see langword="null"/> when none is configured.</param>
public sealed record ScheduleDetailResponse(
    string Name,
    string GroupName,
    string State,
    string ScheduleExpression,
    string? ScheduleExpressionTimezone,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    string TargetArn,
    string RoleArn,
    string FlexibleTimeWindowMode,
    int? MaximumWindowInMinutes,
    string Arn,
    DateTimeOffset? CreationDate,
    DateTimeOffset? LastModificationDate,
    string? TargetInput);

/// <summary>
/// The configuration supplied when creating an EventBridge Scheduler schedule.
/// </summary>
/// <param name="Name">The schedule name, unique within its group.</param>
/// <param name="GroupName">The name of the schedule group the schedule belongs to.</param>
/// <param name="ScheduleExpression">The timing expression that controls when the schedule runs.</param>
/// <param name="ScheduleExpressionTimezone">The timezone in which the schedule expression is evaluated, or <see langword="null"/> to leave unset.</param>
/// <param name="Description">A human-readable description of the schedule, or <see langword="null"/> to leave unset.</param>
/// <param name="StartDate">The earliest moment the schedule may run, or <see langword="null"/> to leave unset.</param>
/// <param name="EndDate">The latest moment the schedule may run, or <see langword="null"/> to leave unset.</param>
/// <param name="TargetArn">The Amazon Resource Name of the target the schedule invokes.</param>
/// <param name="RoleArn">The Amazon Resource Name of the role the scheduler assumes to invoke the target.</param>
/// <param name="FlexibleTimeWindowMode">The flexible time window mode, either <c>OFF</c> or <c>FLEXIBLE</c>.</param>
/// <param name="MaximumWindowInMinutes">The maximum flexible time window in minutes, or <see langword="null"/> when the mode is off.</param>
/// <param name="State">Whether the schedule is enabled or disabled.</param>
/// <param name="TargetInput">A constant JSON payload passed to the target when the schedule runs, or <see langword="null"/> to pass no input.</param>
public sealed record ScheduleCreateRequest(
    string Name,
    string GroupName,
    string ScheduleExpression,
    string? ScheduleExpressionTimezone,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    string TargetArn,
    string RoleArn,
    string FlexibleTimeWindowMode,
    int? MaximumWindowInMinutes,
    string State,
    string? TargetInput);

/// <summary>
/// The configuration supplied when updating an existing EventBridge Scheduler schedule. The schedule
/// name and group are taken from the request route rather than the body.
/// </summary>
/// <param name="ScheduleExpression">The timing expression that controls when the schedule runs.</param>
/// <param name="ScheduleExpressionTimezone">The timezone in which the schedule expression is evaluated, or <see langword="null"/> to leave unset.</param>
/// <param name="Description">A human-readable description of the schedule, or <see langword="null"/> to leave unset.</param>
/// <param name="StartDate">The earliest moment the schedule may run, or <see langword="null"/> to leave unset.</param>
/// <param name="EndDate">The latest moment the schedule may run, or <see langword="null"/> to leave unset.</param>
/// <param name="TargetArn">The Amazon Resource Name of the target the schedule invokes.</param>
/// <param name="RoleArn">The Amazon Resource Name of the role the scheduler assumes to invoke the target.</param>
/// <param name="FlexibleTimeWindowMode">The flexible time window mode, either <c>OFF</c> or <c>FLEXIBLE</c>.</param>
/// <param name="MaximumWindowInMinutes">The maximum flexible time window in minutes, or <see langword="null"/> when the mode is off.</param>
/// <param name="State">Whether the schedule is enabled or disabled.</param>
/// <param name="TargetInput">A constant JSON payload passed to the target when the schedule runs, or <see langword="null"/> to pass no input.</param>
public sealed record ScheduleUpdateRequest(
    string ScheduleExpression,
    string? ScheduleExpressionTimezone,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    string TargetArn,
    string RoleArn,
    string FlexibleTimeWindowMode,
    int? MaximumWindowInMinutes,
    string State,
    string? TargetInput);

/// <summary>
/// The EventBridge Scheduler schedule groups available on the backend.
/// </summary>
/// <param name="Groups">The schedule groups, ordered as returned by the backend.</param>
public sealed record ScheduleGroupListResponse(
    IReadOnlyList<ScheduleGroupResponse> Groups);

/// <summary>
/// A view of an EventBridge Scheduler schedule group.
/// </summary>
/// <param name="Name">The schedule group name, unique within the backend.</param>
/// <param name="State">Whether the schedule group is active or being deleted.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the schedule group.</param>
/// <param name="CreationDate">The moment the schedule group was created, or <see langword="null"/> when not reported.</param>
/// <param name="LastModificationDate">The moment the schedule group was last modified, or <see langword="null"/> when not reported.</param>
public sealed record ScheduleGroupResponse(
    string Name,
    string State,
    string Arn,
    DateTimeOffset? CreationDate,
    DateTimeOffset? LastModificationDate);

/// <summary>
/// The configuration supplied when creating an EventBridge Scheduler schedule group.
/// </summary>
/// <param name="Name">The name of the schedule group to create, unique within the backend.</param>
public sealed record ScheduleGroupCreateRequest(
    string Name);
