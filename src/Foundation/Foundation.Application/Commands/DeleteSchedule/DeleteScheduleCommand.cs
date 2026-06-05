using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteSchedule;

/// <summary>
/// Delete an EventBridge Scheduler schedule by its name and group. This action cannot be undone.
/// </summary>
/// <param name="Name">The name of the schedule to delete.</param>
/// <param name="GroupName">The name of the schedule group the schedule belongs to.</param>
public record DeleteScheduleCommand(
    string Name,
    string GroupName) : ICommand;
