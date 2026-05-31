using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.StepFunctions;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetStateMachine;

internal sealed partial class GetStateMachineQueryHandler
    : IQueryHandler<GetStateMachineQuery, GetStateMachineQueryResult>
{
    private readonly IStepFunctionsClient _client;
    private readonly ILogger _logger;

    public GetStateMachineQueryHandler(
        IStepFunctionsClient client, ILogger<GetStateMachineQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetStateMachineQueryResult>> Handle(
        GetStateMachineQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.StateMachineArn);
        var stateMachine = await _client.DescribeStateMachineAsync(
            request.StateMachineArn, cancellationToken);
        LogHandled(stateMachine.IsSuccess);

        if (!stateMachine.IsSuccess)
        {
            Result<GetStateMachineQueryResult> failure = stateMachine.Error!.Value;
            return failure;
        }

        return new GetStateMachineQueryResult(stateMachine.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Describing Step Functions state machine {StateMachineArn}.")]
    private partial void LogHandling(string stateMachineArn);

    [LoggerMessage(LogLevel.Trace, "Step Functions state machine describe handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
