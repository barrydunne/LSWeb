using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Resilience;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetCircuitStatus;

internal sealed partial class GetCircuitStatusQueryHandler : IQueryHandler<GetCircuitStatusQuery, GetCircuitStatusQueryResult>
{
    private readonly ICircuitBreakerStateProvider _provider;
    private readonly ILogger _logger;

    public GetCircuitStatusQueryHandler(ICircuitBreakerStateProvider provider, ILogger<GetCircuitStatusQueryHandler> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public Task<Result<GetCircuitStatusQueryResult>> Handle(GetCircuitStatusQuery request, CancellationToken cancellationToken)
    {
        var status = _provider.GetStatus();
        LogHandling(status.IsOpen, status.AffectedServices.Count);
        Result<GetCircuitStatusQueryResult> result = new GetCircuitStatusQueryResult(status.IsOpen, status.AffectedServices);
        return Task.FromResult(result);
    }

    [LoggerMessage(LogLevel.Trace, "Handling circuit status query. Open: {IsOpen}. Affected services: {Count}.")]
    private partial void LogHandling(bool isOpen, int count);
}
