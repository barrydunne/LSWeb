using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Search;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetSearchState;

internal sealed partial class GetSearchStateQueryHandler : IQueryHandler<GetSearchStateQuery, GetSearchStateQueryResult>
{
    private readonly ISearchIndexProvider _provider;
    private readonly ISearchIndexSignals _signals;
    private readonly ILogger _logger;

    public GetSearchStateQueryHandler(
        ISearchIndexProvider provider,
        ISearchIndexSignals signals,
        ILogger<GetSearchStateQueryHandler> logger)
    {
        _provider = provider;
        _signals = signals;
        _logger = logger;
    }

    public Task<Result<GetSearchStateQueryResult>> Handle(GetSearchStateQuery request, CancellationToken cancellationToken)
    {
        var snapshot = _provider.GetCurrent();
        var isBuilding = _signals.IsBuilding;
        LogHandling(snapshot.Count, isBuilding);
        Result<GetSearchStateQueryResult> result =
            new GetSearchStateQueryResult(snapshot.BuiltAt, snapshot.Count, isBuilding);
        return Task.FromResult(result);
    }

    [LoggerMessage(LogLevel.Trace, "Handling search state query. Entries: {Count}, building: {IsBuilding}")]
    private partial void LogHandling(int count, bool isBuilding);
}
