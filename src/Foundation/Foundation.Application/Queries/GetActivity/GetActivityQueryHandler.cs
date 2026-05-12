using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetActivity;

internal sealed partial class GetActivityQueryHandler : IQueryHandler<GetActivityQuery, GetActivityQueryResult>
{
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public GetActivityQueryHandler(IActivityLog activityLog, ILogger<GetActivityQueryHandler> logger)
    {
        _activityLog = activityLog;
        _logger = logger;
    }

    public Task<Result<GetActivityQueryResult>> Handle(GetActivityQuery request, CancellationToken cancellationToken)
    {
        var entries = _activityLog.GetEntries();
        LogHandling(entries.Count);
        Result<GetActivityQueryResult> result = new GetActivityQueryResult(entries);
        return Task.FromResult(result);
    }

    [LoggerMessage(LogLevel.Trace, "Handling activity query. Returning {Count} entries.")]
    private partial void LogHandling(int count);
}
