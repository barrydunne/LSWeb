using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.StepFunctions;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListStateMachines;

internal sealed partial class ListStateMachinesQueryHandler
    : IQueryHandler<ListStateMachinesQuery, ListStateMachinesQueryResult>
{
    private readonly IStepFunctionsClient _client;
    private readonly ILogger _logger;

    public ListStateMachinesQueryHandler(
        IStepFunctionsClient client, ILogger<ListStateMachinesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListStateMachinesQueryResult>> Handle(
        ListStateMachinesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var stateMachines = await _client.ListStateMachinesAsync(cancellationToken);
        LogHandled(stateMachines.IsSuccess);

        if (!stateMachines.IsSuccess)
        {
            Result<ListStateMachinesQueryResult> failure = stateMachines.Error!.Value;
            return failure;
        }

        return new ListStateMachinesQueryResult(stateMachines.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Step Functions state machines.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Step Functions state machine list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
