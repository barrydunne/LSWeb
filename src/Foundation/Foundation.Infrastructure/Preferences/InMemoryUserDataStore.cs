using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Preferences;
using Foundation.Domain.Preferences;

namespace Foundation.Infrastructure.Preferences;

/// <summary>
/// Keeps user preferences in process memory only. Used when no host directory is mounted so the
/// container remains stateless; the data is lost when the process stops.
/// </summary>
internal sealed class InMemoryUserDataStore : IUserDataStore
{
    private readonly object _gate = new();
    private UserPreferences _current = UserPreferences.Empty;

    public Task<Result<UserPreferences>> GetPreferencesAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            Result<UserPreferences> result = _current;
            return Task.FromResult(result);
        }
    }

    public Task<Result> SavePreferencesAsync(UserPreferences preferences, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _current = preferences;
        }

        return Task.FromResult(Result.Success());
    }
}
