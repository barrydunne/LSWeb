using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteScheduleGroup;

/// <summary>
/// Delete an EventBridge Scheduler schedule group by its name. The <c>default</c> group cannot be
/// deleted. This action cannot be undone.
/// </summary>
/// <param name="Name">The name of the schedule group to delete.</param>
public record DeleteScheduleGroupCommand(
    string Name) : ICommand;
