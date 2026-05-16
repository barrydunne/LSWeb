using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Preferences;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetFavourites;

internal sealed partial class GetFavouritesQueryHandler : IQueryHandler<GetFavouritesQuery, GetFavouritesQueryResult>
{
    private readonly IUserDataStore _userDataStore;
    private readonly ILogger _logger;

    public GetFavouritesQueryHandler(IUserDataStore userDataStore, ILogger<GetFavouritesQueryHandler> logger)
    {
        _userDataStore = userDataStore;
        _logger = logger;
    }

    public async Task<Result<GetFavouritesQueryResult>> Handle(GetFavouritesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var preferences = await _userDataStore.GetPreferencesAsync(cancellationToken);
        if (!preferences.IsSuccess)
        {
            Result<GetFavouritesQueryResult> failure = preferences.Error!.Value;
            return failure;
        }

        return new GetFavouritesQueryResult(preferences.Value.Favourites);
    }

    [LoggerMessage(LogLevel.Trace, "Handling favourites query.")]
    private partial void LogHandling();
}
