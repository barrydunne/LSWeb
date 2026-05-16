using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Preferences;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetRecentlyViewed;

internal sealed partial class GetRecentlyViewedQueryHandler : IQueryHandler<GetRecentlyViewedQuery, GetRecentlyViewedQueryResult>
{
    private readonly IUserDataStore _userDataStore;
    private readonly ILogger _logger;

    public GetRecentlyViewedQueryHandler(IUserDataStore userDataStore, ILogger<GetRecentlyViewedQueryHandler> logger)
    {
        _userDataStore = userDataStore;
        _logger = logger;
    }

    public async Task<Result<GetRecentlyViewedQueryResult>> Handle(GetRecentlyViewedQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var preferences = await _userDataStore.GetPreferencesAsync(cancellationToken);
        if (!preferences.IsSuccess)
        {
            Result<GetRecentlyViewedQueryResult> failure = preferences.Error!.Value;
            return failure;
        }

        return new GetRecentlyViewedQueryResult(preferences.Value.RecentlyViewed);
    }

    [LoggerMessage(LogLevel.Trace, "Handling recently-viewed query.")]
    private partial void LogHandling();
}
