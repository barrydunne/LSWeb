using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetLiveness;

internal sealed partial class GetLivenessQueryHandler : IQueryHandler<GetLivenessQuery, GetLivenessQueryResult>
{
    private readonly ILogger _logger;

    public GetLivenessQueryHandler(ILogger<GetLivenessQueryHandler> logger)
        => _logger = logger;

    public Task<Result<GetLivenessQueryResult>> Handle(GetLivenessQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        Result<GetLivenessQueryResult> result = new GetLivenessQueryResult("Healthy");
        return Task.FromResult(result);
    }

    [LoggerMessage(LogLevel.Trace, "Handling liveness query.")]
    private partial void LogHandling();
}
