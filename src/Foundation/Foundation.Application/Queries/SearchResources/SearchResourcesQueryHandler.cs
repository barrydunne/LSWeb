using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Search;
using Foundation.Domain.Search;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.SearchResources;

internal sealed partial class SearchResourcesQueryHandler : IQueryHandler<SearchResourcesQuery, SearchResourcesQueryResult>
{
    private readonly ISearchIndexProvider _provider;
    private readonly ILogger _logger;

    public SearchResourcesQueryHandler(ISearchIndexProvider provider, ILogger<SearchResourcesQueryHandler> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public Task<Result<SearchResourcesQueryResult>> Handle(SearchResourcesQuery request, CancellationToken cancellationToken)
    {
        var term = request.Query?.Trim() ?? string.Empty;
        if (term.Length == 0)
        {
            LogHandling(term, 0);
            Result<SearchResourcesQueryResult> empty = new SearchResourcesQueryResult([]);
            return Task.FromResult(empty);
        }

        var matches = _provider.GetCurrent().Entries
            .Where(entry => Matches(entry, term))
            .ToList();

        LogHandling(term, matches.Count);
        Result<SearchResourcesQueryResult> result = new SearchResourcesQueryResult(matches);
        return Task.FromResult(result);
    }

    private static bool Matches(SearchEntry entry, string term)
        => entry.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase)
            || entry.ResourceId.Contains(term, StringComparison.OrdinalIgnoreCase)
            || entry.ServiceKey.Contains(term, StringComparison.OrdinalIgnoreCase);

    [LoggerMessage(LogLevel.Trace, "Handling resource search for '{Term}'. Returning {Count} matches.")]
    private partial void LogHandling(string term, int count);
}
