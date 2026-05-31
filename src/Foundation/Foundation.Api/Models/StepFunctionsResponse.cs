namespace Foundation.Api.Models;

/// <summary>
/// The Step Functions state machines available on the backend.
/// </summary>
/// <param name="StateMachines">The state machine summaries, ordered as returned by the backend.</param>
public sealed record StateMachineListResponse(
    IReadOnlyList<StateMachineSummaryResponse> StateMachines);

/// <summary>
/// A concise view of a Step Functions state machine as it appears in a list.
/// </summary>
/// <param name="Name">The state machine name.</param>
/// <param name="StateMachineArn">The Amazon Resource Name that uniquely identifies the state machine.</param>
/// <param name="Type">The state machine type, either <c>STANDARD</c> or <c>EXPRESS</c>.</param>
/// <param name="CreationDate">The moment the state machine was created.</param>
public sealed record StateMachineSummaryResponse(
    string Name,
    string StateMachineArn,
    string Type,
    DateTimeOffset CreationDate);

/// <summary>
/// The full details of a Step Functions state machine.
/// </summary>
/// <param name="Name">The state machine name.</param>
/// <param name="StateMachineArn">The Amazon Resource Name that uniquely identifies the state machine.</param>
/// <param name="Type">The state machine type, either <c>STANDARD</c> or <c>EXPRESS</c>.</param>
/// <param name="Status">The state machine status, for example <c>ACTIVE</c> or <c>DELETING</c>.</param>
/// <param name="RoleArn">The Amazon Resource Name of the IAM role the state machine assumes.</param>
/// <param name="Definition">The Amazon States Language definition as a JSON document.</param>
/// <param name="CreationDate">The moment the state machine was created.</param>
public sealed record StateMachineDetailResponse(
    string Name,
    string StateMachineArn,
    string Type,
    string Status,
    string RoleArn,
    string Definition,
    DateTimeOffset CreationDate);

/// <summary>
/// The executions of a single Step Functions state machine.
/// </summary>
/// <param name="Executions">The execution summaries, ordered as returned by the backend.</param>
public sealed record ExecutionListResponse(
    IReadOnlyList<ExecutionSummaryResponse> Executions);

/// <summary>
/// A concise view of a Step Functions execution as it appears in a list.
/// </summary>
/// <param name="ExecutionArn">The Amazon Resource Name that uniquely identifies the execution.</param>
/// <param name="Name">The execution name.</param>
/// <param name="StateMachineArn">The Amazon Resource Name of the state machine the execution belongs to.</param>
/// <param name="Status">The execution status, for example <c>RUNNING</c>, <c>SUCCEEDED</c>, or <c>FAILED</c>.</param>
/// <param name="StartDate">The moment the execution started.</param>
/// <param name="StopDate">The moment the execution stopped, or <see langword="null"/> when it is still running.</param>
public sealed record ExecutionSummaryResponse(
    string ExecutionArn,
    string Name,
    string StateMachineArn,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset? StopDate);

/// <summary>
/// A request to start a Step Functions execution.
/// </summary>
/// <param name="StateMachineArn">The Amazon Resource Name of the state machine to execute.</param>
/// <param name="Name">An optional name for the execution; the backend generates one when omitted.</param>
/// <param name="Input">An optional JSON document passed as the execution input.</param>
public sealed record StartExecutionRequest(
    string StateMachineArn,
    string? Name,
    string? Input);

/// <summary>
/// The result of starting a Step Functions execution.
/// </summary>
/// <param name="ExecutionArn">The Amazon Resource Name that uniquely identifies the new execution.</param>
/// <param name="StartDate">The moment the execution started.</param>
public sealed record StartExecutionResponse(
    string ExecutionArn,
    DateTimeOffset StartDate);

/// <summary>
/// The ordered history of a single Step Functions execution.
/// </summary>
/// <param name="Events">The history events, ordered as returned by the backend.</param>
public sealed record ExecutionHistoryResponse(
    IReadOnlyList<ExecutionHistoryEventResponse> Events);

/// <summary>
/// A single event in the history of a Step Functions execution.
/// </summary>
/// <param name="Id">The identifier of the event within the execution history.</param>
/// <param name="PreviousEventId">The identifier of the preceding event, or <see langword="null"/> for the first event.</param>
/// <param name="Type">The event type, for example <c>TaskStateEntered</c> or <c>ExecutionSucceeded</c>.</param>
/// <param name="Timestamp">The moment the event occurred.</param>
/// <param name="Name">The state name associated with the event, where one applies.</param>
/// <param name="Input">The JSON input recorded for the event, where one applies.</param>
/// <param name="Output">The JSON output recorded for the event, where one applies.</param>
/// <param name="Error">The error name recorded for a failure event, where one applies.</param>
/// <param name="Cause">The failure cause recorded for a failure event, where one applies.</param>
public sealed record ExecutionHistoryEventResponse(
    long Id,
    long? PreviousEventId,
    string Type,
    DateTimeOffset Timestamp,
    string? Name,
    string? Input,
    string? Output,
    string? Error,
    string? Cause);
