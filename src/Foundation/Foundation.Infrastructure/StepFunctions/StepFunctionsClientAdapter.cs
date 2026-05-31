using System.Diagnostics.CodeAnalysis;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.StepFunctions;
using Foundation.Domain.StepFunctions;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.StepFunctions;

/// <summary>
/// Reads Step Functions through the resilient AWS gateway so the same code works against LocalStack
/// or real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and
/// converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class StepFunctionsClientAdapter : IStepFunctionsClient
{
    private const string ServiceKey = "step-functions";

    private readonly IAwsGateway _gateway;

    public StepFunctionsClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<StateMachineSummary>>> ListStateMachinesAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonStepFunctionsClient, IReadOnlyList<StateMachineSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var stateMachines = new List<StateMachineSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListStateMachinesAsync(
                        new ListStateMachinesRequest { NextToken = nextToken },
                        token);

                    foreach (var stateMachine in response.StateMachines ?? [])
                        stateMachines.Add(ToSummary(stateMachine));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return stateMachines;
            },
            cancellationToken);

    public Task<Result<StateMachineDetail>> DescribeStateMachineAsync(
        string stateMachineArn, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonStepFunctionsClient, StateMachineDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.DescribeStateMachineAsync(
                    new DescribeStateMachineRequest { StateMachineArn = stateMachineArn },
                    token);

                return new StateMachineDetail(
                    response.Name ?? string.Empty,
                    response.StateMachineArn ?? string.Empty,
                    response.Type?.Value ?? string.Empty,
                    response.Status?.Value ?? string.Empty,
                    response.RoleArn ?? string.Empty,
                    response.Definition ?? string.Empty,
                    response.CreationDate ?? default);
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<ExecutionSummary>>> ListExecutionsAsync(
        string stateMachineArn, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonStepFunctionsClient, IReadOnlyList<ExecutionSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var executions = new List<ExecutionSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListExecutionsAsync(
                        new ListExecutionsRequest
                        {
                            StateMachineArn = stateMachineArn,
                            NextToken = nextToken,
                        },
                        token);

                    foreach (var execution in response.Executions ?? [])
                        executions.Add(ToSummary(execution));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return executions;
            },
            cancellationToken);

    public Task<Result<ExecutionStartResult>> StartExecutionAsync(
        string stateMachineArn, string? name, string? input, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonStepFunctionsClient, ExecutionStartResult>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.StartExecutionAsync(
                    new StartExecutionRequest
                    {
                        StateMachineArn = stateMachineArn,
                        Name = string.IsNullOrWhiteSpace(name) ? null : name,
                        Input = string.IsNullOrWhiteSpace(input) ? null : input,
                    },
                    token);

                return new ExecutionStartResult(
                    response.ExecutionArn ?? string.Empty,
                    response.StartDate ?? default);
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<ExecutionHistoryEvent>>> GetExecutionHistoryAsync(
        string executionArn, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonStepFunctionsClient, IReadOnlyList<ExecutionHistoryEvent>>(
            ServiceKey,
            async (client, token) =>
            {
                var events = new List<ExecutionHistoryEvent>();
                string? nextToken = null;

                do
                {
                    var response = await client.GetExecutionHistoryAsync(
                        new GetExecutionHistoryRequest
                        {
                            ExecutionArn = executionArn,
                            IncludeExecutionData = true,
                            NextToken = nextToken,
                        },
                        token);

                    foreach (var historyEvent in response.Events ?? [])
                        events.Add(ToEvent(historyEvent));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return events;
            },
            cancellationToken);

    private static StateMachineSummary ToSummary(StateMachineListItem stateMachine)
        => new(
            stateMachine.Name ?? string.Empty,
            stateMachine.StateMachineArn ?? string.Empty,
            stateMachine.Type?.Value ?? string.Empty,
            stateMachine.CreationDate ?? default);

    private static ExecutionSummary ToSummary(ExecutionListItem execution)
        => new(
            execution.ExecutionArn ?? string.Empty,
            execution.Name ?? string.Empty,
            execution.StateMachineArn ?? string.Empty,
            execution.Status?.Value ?? string.Empty,
            execution.StartDate ?? default,
            execution.StopDate);

    private static ExecutionHistoryEvent ToEvent(HistoryEvent historyEvent)
        => new(
            historyEvent.Id ?? 0,
            historyEvent.PreviousEventId is null or 0 ? null : historyEvent.PreviousEventId,
            historyEvent.Type?.Value ?? string.Empty,
            historyEvent.Timestamp ?? default,
            historyEvent.StateEnteredEventDetails?.Name
                ?? historyEvent.StateExitedEventDetails?.Name,
            historyEvent.StateEnteredEventDetails?.Input
                ?? historyEvent.ExecutionStartedEventDetails?.Input,
            historyEvent.StateExitedEventDetails?.Output
                ?? historyEvent.ExecutionSucceededEventDetails?.Output
                ?? historyEvent.TaskSucceededEventDetails?.Output
                ?? historyEvent.TaskSubmittedEventDetails?.Output
                ?? historyEvent.LambdaFunctionSucceededEventDetails?.Output
                ?? historyEvent.ActivitySucceededEventDetails?.Output,
            historyEvent.ExecutionFailedEventDetails?.Error
                ?? historyEvent.ExecutionAbortedEventDetails?.Error
                ?? historyEvent.ExecutionTimedOutEventDetails?.Error
                ?? historyEvent.TaskFailedEventDetails?.Error
                ?? historyEvent.TaskTimedOutEventDetails?.Error
                ?? historyEvent.LambdaFunctionFailedEventDetails?.Error
                ?? historyEvent.LambdaFunctionTimedOutEventDetails?.Error
                ?? historyEvent.ActivityFailedEventDetails?.Error
                ?? historyEvent.ActivityTimedOutEventDetails?.Error,
            historyEvent.ExecutionFailedEventDetails?.Cause
                ?? historyEvent.ExecutionAbortedEventDetails?.Cause
                ?? historyEvent.ExecutionTimedOutEventDetails?.Cause
                ?? historyEvent.TaskFailedEventDetails?.Cause
                ?? historyEvent.TaskTimedOutEventDetails?.Cause
                ?? historyEvent.LambdaFunctionFailedEventDetails?.Cause
                ?? historyEvent.LambdaFunctionTimedOutEventDetails?.Cause
                ?? historyEvent.ActivityFailedEventDetails?.Cause
                ?? historyEvent.ActivityTimedOutEventDetails?.Cause);
}
