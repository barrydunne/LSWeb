namespace Foundation.Domain.Scheduler;

/// <summary>
/// A concise view of an EventBridge Scheduler schedule as it appears in a schedule list. The list
/// view does not include the schedule's expression or target configuration; those are read from the
/// schedule detail.
/// </summary>
/// <param name="Name">The name of the schedule, unique within its group.</param>
/// <param name="GroupName">The name of the schedule group the schedule belongs to.</param>
/// <param name="State">Whether the schedule is enabled or disabled.</param>
/// <param name="TargetArn">The Amazon Resource Name of the target the schedule invokes.</param>
/// <param name="Arn">The Amazon Resource Name of the schedule itself.</param>
public sealed record ScheduleSummary(
    string Name,
    string GroupName,
    string State,
    string TargetArn,
    string Arn);
