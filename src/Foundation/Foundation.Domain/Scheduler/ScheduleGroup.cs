namespace Foundation.Domain.Scheduler;

/// <summary>
/// A schedule group that namespaces EventBridge Scheduler schedules. The <c>default</c> group always
/// exists and cannot be deleted.
/// </summary>
/// <param name="Name">The name of the schedule group, unique within the backend.</param>
/// <param name="State">Whether the schedule group is active or being deleted.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the schedule group.</param>
/// <param name="CreationDate">The moment the schedule group was created, or <see langword="null"/> when not reported.</param>
/// <param name="LastModificationDate">The moment the schedule group was last modified, or <see langword="null"/> when not reported.</param>
public sealed record ScheduleGroup(
    string Name,
    string State,
    string Arn,
    DateTimeOffset? CreationDate,
    DateTimeOffset? LastModificationDate);
