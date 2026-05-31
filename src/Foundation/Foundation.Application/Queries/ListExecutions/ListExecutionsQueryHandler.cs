using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.StepFunctions;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListExecutions;

internal sealed partial class ListExecutionsQueryHandler
    : IQueryHandler<ListExecutionsQuery, ListExecutionsQueryResult>
{
    private readonly IStepFunctionsClient _client;
    private readonly ILogger _logger;

    public ListExecutionsQueryHandler(
        IStepFunctionsClient client, ILogger<ListExecutionsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListExecutionsQueryResult>> Handle(
        ListExecutionsQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var executions = await _client.ListExecutionsAsync(request.StateMachineArn, cancellationToken);
        LogHandled(executions.IsSuccess);

        if (!executions.IsSuccess)
        {
            Result<ListExecutionsQueryResult> failure = executions.Error!.Value;
            return failure;
        }

        return new ListExecutionsQueryResult(executions.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Step Functions executions.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Step Functions execution list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
