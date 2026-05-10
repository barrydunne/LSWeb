using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Health;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetHealth;

internal sealed partial class GetHealthQueryHandler : IQueryHandler<GetHealthQuery, GetHealthQueryResult>
{
    private readonly IHealthStatusProvider _provider;
    private readonly ILogger _logger;

    public GetHealthQueryHandler(IHealthStatusProvider provider, ILogger<GetHealthQueryHandler> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public Task<Result<GetHealthQueryResult>> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        var snapshot = _provider.GetCurrent();
        LogHandling(snapshot.Services.Count);
        Result<GetHealthQueryResult> result = new GetHealthQueryResult(snapshot.Services);
        return Task.FromResult(result);
    }

    [LoggerMessage(LogLevel.Trace, "Handling health query. Returning {Count} services.")]
    private partial void LogHandling(int count);
}
