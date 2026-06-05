using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Scheduler;

namespace Foundation.Application.Scheduler;

/// <summary>
/// Provides access to EventBridge Scheduler schedules on the configured backend.
/// </summary>
public interface ISchedulerClient
{
    /// <summary>
    /// Lists the schedules across all schedule groups.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The schedules, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<ScheduleSummary>>> ListSchedulesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the full configuration of a single schedule.
    /// </summary>
    /// <param name="name">The name of the schedule.</param>
    /// <param name="groupName">The name of the schedule group the schedule belongs to.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The schedule detail, or an error if the schedule could not be read.</returns>
    Task<Result<ScheduleDetail>> GetScheduleAsync(string name, string groupName, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new schedule from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the schedule to create.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the schedule could not be created.</returns>
    Task<Result> CreateScheduleAsync(ScheduleSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing schedule to match the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the schedule to update.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the schedule could not be updated.</returns>
    Task<Result> UpdateScheduleAsync(ScheduleSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a schedule by its name and group.
    /// </summary>
    /// <param name="name">The name of the schedule to delete.</param>
    /// <param name="groupName">The name of the schedule group the schedule belongs to.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the schedule could not be deleted.</returns>
    Task<Result> DeleteScheduleAsync(string name, string groupName, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the schedule groups available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The schedule groups, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<ScheduleGroup>>> ListScheduleGroupsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new schedule group with the supplied name.
    /// </summary>
    /// <param name="name">The name of the schedule group to create.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the schedule group could not be created.</returns>
    Task<Result> CreateScheduleGroupAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a schedule group by its name. The <c>default</c> group cannot be deleted.
    /// </summary>
    /// <param name="name">The name of the schedule group to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the schedule group could not be deleted.</returns>
    Task<Result> DeleteScheduleGroupAsync(string name, CancellationToken cancellationToken);
}
