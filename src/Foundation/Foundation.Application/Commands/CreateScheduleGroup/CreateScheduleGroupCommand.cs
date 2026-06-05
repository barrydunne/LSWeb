using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateScheduleGroup;

/// <summary>
/// Create an EventBridge Scheduler schedule group with the supplied name.
/// </summary>
/// <param name="Name">The name of the schedule group to create, unique within the backend.</param>
public record CreateScheduleGroupCommand(
    string Name) : ICommand;
