using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListLambdaTestEvents;

internal sealed partial class ListLambdaTestEventsQueryHandler : IQueryHandler<ListLambdaTestEventsQuery, ListLambdaTestEventsQueryResult>
{
    private readonly ITestEventStore _store;
    private readonly ILogger _logger;

    public ListLambdaTestEventsQueryHandler(ITestEventStore store, ILogger<ListLambdaTestEventsQueryHandler> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<Result<ListLambdaTestEventsQueryResult>> Handle(ListLambdaTestEventsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var events = await _store.GetEventsAsync(request.FunctionName, cancellationToken);
        LogHandled(events.IsSuccess);

        if (!events.IsSuccess)
        {
            Result<ListLambdaTestEventsQueryResult> failure = events.Error!.Value;
            return failure;
        }

        var ordered = events.Value
            .OrderBy(_ => _.Name, StringComparer.Ordinal)
            .ToList();

        return new ListLambdaTestEventsQueryResult(ordered, LambdaTestEventTemplates.Templates);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Lambda test events for {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda test event listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
