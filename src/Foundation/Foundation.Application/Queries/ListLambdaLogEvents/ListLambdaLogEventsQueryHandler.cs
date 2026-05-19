using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListLambdaLogEvents;

internal sealed partial class ListLambdaLogEventsQueryHandler : IQueryHandler<ListLambdaLogEventsQuery, ListLambdaLogEventsQueryResult>
{
    private const int DefaultLimit = 100;
    private const int MaxLimit = 1000;

    private readonly ILambdaClient _client;
    private readonly ILogger _logger;

    public ListLambdaLogEventsQueryHandler(ILambdaClient client, ILogger<ListLambdaLogEventsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListLambdaLogEventsQueryResult>> Handle(ListLambdaLogEventsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var limit = request.Limit <= 0 ? DefaultLimit : Math.Min(request.Limit, MaxLimit);
        var events = await _client.GetRecentLogEventsAsync(request.FunctionName, limit, cancellationToken);
        LogHandled(events.IsSuccess);

        if (!events.IsSuccess)
        {
            Result<ListLambdaLogEventsQueryResult> failure = events.Error!.Value;
            return failure;
        }

        var ordered = events.Value
            .OrderBy(_ => _.Timestamp, StringComparer.Ordinal)
            .ToList();

        return new ListLambdaLogEventsQueryResult($"/aws/lambda/{request.FunctionName}", ordered);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Lambda log events for {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda log event listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
