using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Lambda;

/// <summary>
/// Loads and persists named test events per Lambda function without the application layer
/// depending on where the data is kept. Implementations store the data on a mounted host directory
/// when one is configured and fall back to volatile in-memory storage so the container stays
/// stateless by default.
/// </summary>
public interface ITestEventStore
{
    /// <summary>
    /// Get the saved test events for a function, returning an empty list when none have been saved.
    /// </summary>
    /// <param name="functionName">The name of the function whose events to read.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation.</param>
    /// <returns>The saved events, or a failure describing why they could not be read.</returns>
    Task<Result<IReadOnlyList<LambdaTestEvent>>> GetEventsAsync(string functionName, CancellationToken cancellationToken);

    /// <summary>
    /// Persist a test event for a function, replacing any existing event with the same name.
    /// </summary>
    /// <param name="functionName">The name of the function the event belongs to.</param>
    /// <param name="testEvent">The event to save.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation.</param>
    /// <returns>A successful result, or a failure describing why the event could not be saved.</returns>
    Task<Result> SaveEventAsync(string functionName, LambdaTestEvent testEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a named test event for a function. Deleting an unknown event succeeds silently.
    /// </summary>
    /// <param name="functionName">The name of the function the event belongs to.</param>
    /// <param name="name">The name of the event to delete.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation.</param>
    /// <returns>A successful result, or a failure describing why the event could not be deleted.</returns>
    Task<Result> DeleteEventAsync(string functionName, string name, CancellationToken cancellationToken);
}
