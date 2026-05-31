using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.StepFunctions;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetExecutionHistory;

internal sealed partial class GetExecutionHistoryQueryHandler
    : IQueryHandler<GetExecutionHistoryQuery, GetExecutionHistoryQueryResult>
{
    private readonly IStepFunctionsClient _client;
    private readonly ILogger _logger;

    public GetExecutionHistoryQueryHandler(
        IStepFunctionsClient client, ILogger<GetExecutionHistoryQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetExecutionHistoryQueryResult>> Handle(
        GetExecutionHistoryQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var events = await _client.GetExecutionHistoryAsync(request.ExecutionArn, cancellationToken);
        LogHandled(events.IsSuccess);

        if (!events.IsSuccess)
        {
            Result<GetExecutionHistoryQueryResult> failure = events.Error!.Value;
            return failure;
        }

        return new GetExecutionHistoryQueryResult(events.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting Step Functions execution history.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Step Functions execution history handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
