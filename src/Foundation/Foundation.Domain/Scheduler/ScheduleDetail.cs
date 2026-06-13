namespace Foundation.Domain.Scheduler;

/// <summary>
/// The full configuration of an EventBridge Scheduler schedule, including its timing expression,
/// flexible time window and target. The informational values describe the schedule's identity and
/// lifecycle and cannot be changed directly.
/// </summary>
/// <param name="Name">The name of the schedule, unique within its group.</param>
/// <param name="GroupName">The name of the schedule group the schedule belongs to.</param>
/// <param name="State">Whether the schedule is enabled or disabled.</param>
/// <param name="ScheduleExpression">The timing expression (<c>rate()</c>, <c>cron()</c> or <c>at()</c>) that controls when the schedule runs.</param>
/// <param name="ScheduleExpressionTimezone">The timezone in which the schedule expression is evaluated, if specified.</param>
/// <param name="Description">A human-readable description of the schedule, if specified.</param>
/// <param name="StartDate">The earliest moment the schedule may run, if specified.</param>
/// <param name="EndDate">The latest moment the schedule may run, if specified.</param>
/// <param name="TargetArn">The Amazon Resource Name of the target the schedule invokes.</param>
/// <param name="RoleArn">The Amazon Resource Name of the role the scheduler assumes to invoke the target.</param>
/// <param name="FlexibleTimeWindowMode">The flexible time window mode (<c>OFF</c> or <c>FLEXIBLE</c>).</param>
/// <param name="MaximumWindowInMinutes">The maximum flexible time window, in minutes, when the mode is flexible.</param>
/// <param name="Arn">The Amazon Resource Name of the schedule itself.</param>
/// <param name="CreationDate">The moment the schedule was created, if reported.</param>
/// <param name="LastModificationDate">The moment the schedule was last modified, if reported.</param>
/// <param name="TargetInput">A constant JSON payload passed to the target when the schedule runs, if specified.</param>
public sealed record ScheduleDetail(
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
