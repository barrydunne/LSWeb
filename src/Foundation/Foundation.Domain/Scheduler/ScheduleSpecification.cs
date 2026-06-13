namespace Foundation.Domain.Scheduler;

/// <summary>
/// The desired configuration of an EventBridge Scheduler schedule, used when creating or updating it.
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
public record ScheduleSpecification(
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
