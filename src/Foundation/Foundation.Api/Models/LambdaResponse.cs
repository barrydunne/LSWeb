namespace Foundation.Api.Models;

/// <summary>
/// The Lambda functions available on the configured backend.
/// </summary>
/// <param name="Functions">The function summaries, ordered as returned by the backend.</param>
public sealed record LambdaFunctionListResponse(IReadOnlyList<LambdaFunctionSummaryResponse> Functions);

/// <summary>
/// A concise view of a Lambda function as it appears in a function list.
/// </summary>
/// <param name="FunctionName">The unique name of the function.</param>
/// <param name="Runtime">The managed runtime identifier, for example <c>python3.12</c>.</param>
/// <param name="Description">The function description; empty when none is set.</param>
/// <param name="LastModified">The timestamp the function configuration was last updated.</param>
/// <param name="MemorySize">The memory allocated to the function in megabytes.</param>
/// <param name="Timeout">The function execution timeout in seconds.</param>
public sealed record LambdaFunctionSummaryResponse(
    string FunctionName,
    string Runtime,
    string Description,
    string LastModified,
    int MemorySize,
    int Timeout);

/// <summary>
/// The full configuration of a single Lambda function.
/// </summary>
/// <param name="FunctionName">The unique name of the function.</param>
/// <param name="FunctionArn">The Amazon Resource Name identifying the function.</param>
/// <param name="Runtime">The managed runtime identifier, for example <c>python3.12</c>.</param>
/// <param name="Handler">The entry point the runtime invokes.</param>
/// <param name="Description">The function description; empty when none is set.</param>
/// <param name="LastModified">The timestamp the function configuration was last updated.</param>
/// <param name="MemorySize">The memory allocated to the function in megabytes.</param>
/// <param name="Timeout">The function execution timeout in seconds.</param>
/// <param name="Role">The execution role ARN assumed by the function.</param>
public sealed record LambdaFunctionResponse(
    string FunctionName,
    string FunctionArn,
    string Runtime,
    string Handler,
    string Description,
    string LastModified,
    int MemorySize,
    int Timeout,
    string Role);

/// <summary>
/// The environment variables of a Lambda function, with sensitive values masked as required.
/// </summary>
/// <param name="Variables">The environment variables ordered by name.</param>
/// <param name="RevealAllowed">Whether the host permits sensitive values to be revealed.</param>
public sealed record LambdaEnvironmentResponse(
    IReadOnlyList<LambdaEnvironmentVariableResponse> Variables,
    bool RevealAllowed);

/// <summary>
/// A single Lambda environment variable as presented to the client.
/// </summary>
/// <param name="Name">The environment variable name.</param>
/// <param name="Value">The value to display; sensitive values are masked unless a reveal was permitted.</param>
/// <param name="IsSensitive">Whether the variable is considered sensitive.</param>
public sealed record LambdaEnvironmentVariableResponse(
    string Name,
    string Value,
    bool IsSensitive);

/// <summary>
/// A request to replace the environment variables of a Lambda function.
/// </summary>
/// <param name="Variables">The full set of environment variables to apply.</param>
public sealed record LambdaEnvironmentUpdateRequest(
    IReadOnlyList<LambdaEnvironmentVariableRequest> Variables);

/// <summary>
/// A single environment variable supplied when updating a Lambda function.
/// </summary>
/// <param name="Name">The environment variable name.</param>
/// <param name="Value">The value to apply; the mask sentinel preserves the existing stored value.</param>
public sealed record LambdaEnvironmentVariableRequest(
    string Name,
    string Value);

/// <summary>
/// A request to invoke a Lambda function synchronously.
/// </summary>
/// <param name="Payload">The JSON payload to send to the function; defaults to an empty object when omitted.</param>
public sealed record LambdaInvokeRequest(string? Payload);

/// <summary>
/// A request to create a new Lambda function.
/// </summary>
/// <param name="FunctionName">The unique name of the function to create.</param>
/// <param name="Runtime">The managed runtime identifier, for example <c>python3.12</c>.</param>
/// <param name="Handler">The entry point the runtime invokes.</param>
/// <param name="Role">The execution role ARN the function assumes.</param>
/// <param name="Description">The function description; treated as empty when omitted.</param>
/// <param name="MemorySize">The memory to allocate to the function in megabytes.</param>
/// <param name="Timeout">The function execution timeout in seconds.</param>
/// <param name="ZipFileBase64">The deployment package as a base64-encoded ZIP archive.</param>
public sealed record LambdaFunctionCreateRequest(
    string FunctionName,
    string Runtime,
    string Handler,
    string Role,
    string? Description,
    int MemorySize,
    int Timeout,
    string ZipFileBase64);

/// <summary>
/// A request to update an existing Lambda function's configuration and, optionally, its code.
/// </summary>
/// <param name="Runtime">The managed runtime identifier, for example <c>python3.12</c>.</param>
/// <param name="Handler">The entry point the runtime invokes.</param>
/// <param name="Role">The execution role ARN the function assumes.</param>
/// <param name="Description">The function description; treated as empty when omitted.</param>
/// <param name="MemorySize">The memory to allocate to the function in megabytes.</param>
/// <param name="Timeout">The function execution timeout in seconds.</param>
/// <param name="ZipFileBase64">An optional replacement deployment package as a base64-encoded ZIP archive; the code is left unchanged when omitted.</param>
public sealed record LambdaFunctionUpdateRequest(
    string Runtime,
    string Handler,
    string Role,
    string? Description,
    int MemorySize,
    int Timeout,
    string? ZipFileBase64);

/// <summary>
/// The result of invoking a Lambda function synchronously.
/// </summary>
/// <param name="StatusCode">The HTTP status code returned by the Lambda service; 200 indicates the function ran.</param>
/// <param name="Payload">The response payload returned by the function, as a UTF-8 string; empty when none was returned.</param>
/// <param name="LogTail">The tail of the execution log, including the billed-duration report line; empty when none was returned.</param>
/// <param name="FunctionError">The error type reported by Lambda, for example <c>Unhandled</c>; empty when the function executed successfully.</param>
/// <param name="DurationMs">The wall-clock duration of the invocation in milliseconds.</param>
public sealed record LambdaInvocationResponse(
    int StatusCode,
    string Payload,
    string LogTail,
    string FunctionError,
    long DurationMs);

/// <summary>
/// The saved test events for a Lambda function together with the starter templates available to
/// seed new events.
/// </summary>
/// <param name="Events">The events the user has saved for the function, ordered by name.</param>
/// <param name="Templates">The fixed starter templates available to all functions.</param>
public sealed record LambdaTestEventListResponse(
    IReadOnlyList<LambdaTestEventResponse> Events,
    IReadOnlyList<LambdaTestEventResponse> Templates);

/// <summary>
/// A named, reusable test-event payload for a Lambda function.
/// </summary>
/// <param name="Name">The name that identifies the test event.</param>
/// <param name="Payload">The JSON payload sent when the event is invoked.</param>
public sealed record LambdaTestEventResponse(
    string Name,
    string Payload);

/// <summary>
/// A request to save a named test event for a Lambda function.
/// </summary>
/// <param name="Name">The name that identifies the test event within the function.</param>
/// <param name="Payload">The JSON payload to store; defaults to an empty object when omitted.</param>
public sealed record LambdaTestEventSaveRequest(
    string Name,
    string? Payload);

/// <summary>
/// The event source mappings and S3 triggers configured for a Lambda function.
/// </summary>
/// <param name="Mappings">The event source mappings, ordered by their source ARN.</param>
/// <param name="S3Triggers">The S3 buckets configured to trigger the function, ordered by their bucket ARN.</param>
public sealed record LambdaEventSourceMappingListResponse(
    IReadOnlyList<LambdaEventSourceMappingResponse> Mappings,
    IReadOnlyList<LambdaS3TriggerResponse> S3Triggers);

/// <summary>
/// An S3 bucket configured to trigger a Lambda function, surfaced from the function's
/// resource-based policy.
/// </summary>
/// <param name="BucketArn">The Amazon Resource Name of the source bucket.</param>
public sealed record LambdaS3TriggerResponse(
    string BucketArn);

/// <summary>
/// A single event source mapping linking an event source to the Lambda function it triggers.
/// </summary>
/// <param name="Uuid">The unique identifier of the mapping.</param>
/// <param name="EventSourceArn">The Amazon Resource Name of the event source; empty when not reported.</param>
/// <param name="FunctionArn">The Amazon Resource Name of the target function; empty when not reported.</param>
/// <param name="State">The mapping state reported by AWS, for example <c>Enabled</c> or <c>Disabled</c>; empty when not reported.</param>
/// <param name="BatchSize">The maximum number of records delivered to the function in a single batch.</param>
/// <param name="LastModified">The timestamp the mapping was last updated, in ISO 8601 form; empty when not reported.</param>
public sealed record LambdaEventSourceMappingResponse(
    string Uuid,
    string EventSourceArn,
    string FunctionArn,
    string State,
    int BatchSize,
    string LastModified);

/// <summary>
/// A request to enable or disable a Lambda event source mapping.
/// </summary>
/// <param name="Enabled">Whether the mapping should be enabled.</param>
public sealed record LambdaEventSourceMappingStateRequest(
    bool Enabled);

/// <summary>
/// The most recent CloudWatch log events for a Lambda function.
/// </summary>
/// <param name="LogGroupName">The CloudWatch log group the events were read from.</param>
/// <param name="Events">The log events, ordered oldest first.</param>
public sealed record LambdaLogEventListResponse(
    string LogGroupName,
    IReadOnlyList<LambdaLogEventResponse> Events);

/// <summary>
/// A single CloudWatch log event emitted by a Lambda function.
/// </summary>
/// <param name="Timestamp">The time the event was recorded, in ISO 8601 form; empty when not reported.</param>
/// <param name="Message">The raw log message line.</param>
/// <param name="LogStreamName">The name of the log stream the event belongs to; empty when not reported.</param>
public sealed record LambdaLogEventResponse(
    string Timestamp,
    string Message,
    string LogStreamName);

/// <summary>
/// Derived invocation monitoring information for a Lambda function.
/// </summary>
/// <param name="LogGroupName">The CloudWatch log group the events were derived from.</param>
/// <param name="Metrics">The aggregate metrics derived from the recent invocations.</param>
/// <param name="RecentInvocations">The recent invocations, ordered newest first.</param>
public sealed record LambdaInvocationInsightsResponse(
    string LogGroupName,
    LambdaInvocationMetricsResponse Metrics,
    IReadOnlyList<LambdaRecentInvocationResponse> RecentInvocations);

/// <summary>
/// Aggregate metrics derived from a Lambda function's recent invocations.
/// </summary>
/// <param name="InvocationCount">The number of completed invocations observed.</param>
/// <param name="ErrorCount">The number of observed invocations that reported an error.</param>
/// <param name="AverageDurationMs">The mean execution duration in milliseconds; zero when no invocations were observed.</param>
/// <param name="MaxDurationMs">The longest execution duration in milliseconds; zero when no invocations were observed.</param>
public sealed record LambdaInvocationMetricsResponse(
    int InvocationCount,
    int ErrorCount,
    double AverageDurationMs,
    double MaxDurationMs);

/// <summary>
/// A single completed Lambda invocation derived from its CloudWatch log events.
/// </summary>
/// <param name="RequestId">The Lambda request identifier for the invocation.</param>
/// <param name="Timestamp">The time the invocation completed, in ISO 8601 form; empty when not reported.</param>
/// <param name="DurationMs">The reported execution duration in milliseconds.</param>
/// <param name="HasError">Whether the invocation's log events reported an error.</param>
public sealed record LambdaRecentInvocationResponse(
    string RequestId,
    string Timestamp,
    double DurationMs,
    bool HasError);

/// <summary>
/// The layer versions attached to a Lambda function.
/// </summary>
/// <param name="Layers">The attached layers, ordered by their ARN.</param>
public sealed record LambdaLayerListResponse(
    IReadOnlyList<LambdaLayerResponse> Layers);

/// <summary>
/// A single layer version attached to a Lambda function.
/// </summary>
/// <param name="Arn">The full Amazon Resource Name of the attached layer version; empty when not reported.</param>
/// <param name="Name">The layer name derived from the ARN; empty when it cannot be derived.</param>
/// <param name="Version">The layer version derived from the ARN; empty when it cannot be derived.</param>
public sealed record LambdaLayerResponse(
    string Arn,
    string Name,
    string Version);
