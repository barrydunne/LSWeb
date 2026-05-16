using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Preferences;

namespace Foundation.Application.Preferences;

/// <summary>
/// Loads and persists user preferences without the application layer depending on where the data
/// is kept. Implementations store the data on a mounted host directory when one is configured and
/// fall back to volatile in-memory storage so the container stays stateless by default.
/// </summary>
public interface IUserDataStore
{
    /// <summary>
    /// Get the stored preferences, returning <see cref="UserPreferences.Empty"/> when nothing has
    /// been saved yet.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation.</param>
    /// <returns>The stored preferences, or a failure describing why they could not be read.</returns>
    Task<Result<UserPreferences>> GetPreferencesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Persist the supplied preferences, replacing anything previously stored.
    /// </summary>
    /// <param name="preferences">The preferences to persist.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation.</param>
    /// <returns>A successful result, or a failure describing why the data could not be saved.</returns>
    Task<Result> SavePreferencesAsync(UserPreferences preferences, CancellationToken cancellationToken);
}
