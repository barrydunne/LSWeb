using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateLambdaFunction;
using Foundation.Application.Commands.DeleteLambdaFunction;
using Foundation.Application.Commands.DeleteLambdaTestEvent;
using Foundation.Application.Commands.InvokeLambdaFunction;
using Foundation.Application.Commands.SaveLambdaTestEvent;
using Foundation.Application.Commands.SetLambdaEventSourceMappingState;
using Foundation.Application.Commands.UpdateLambdaEnvironment;
using Foundation.Application.Commands.UpdateLambdaFunction;
using Foundation.Application.Queries.GetLambdaEnvironment;
using Foundation.Application.Queries.GetLambdaFunction;
using Foundation.Application.Queries.GetLambdaInvocationInsights;
using Foundation.Application.Queries.ListLambdaEventSourceMappings;
using Foundation.Application.Queries.ListLambdaFunctions;
using Foundation.Application.Queries.ListLambdaLayers;
using Foundation.Application.Queries.ListLambdaLogEvents;
using Foundation.Application.Queries.ListLambdaTestEvents;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides read access to AWS Lambda functions: listing the available functions and viewing the
/// full configuration of a single function.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/lambda")]
public partial class LambdaController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries.</param>
    /// <param name="logger">The logger.</param>
    public LambdaController(ISender sender, ILogger<LambdaController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the Lambda functions available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the function summaries.</returns>
    [HttpGet("functions")]
    [ProducesResponseType(typeof(LambdaFunctionListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListFunctions(CancellationToken cancellationToken)
    {
        LogHandlingList();
        var result = await _sender.Send(new ListLambdaFunctionsQuery(), cancellationToken);
        LogListHandled(result.IsSuccess);
        return result.Match(
            functions => Results.Ok(new LambdaFunctionListResponse(
                functions.Functions
                    .Select(function => new LambdaFunctionSummaryResponse(
                        function.FunctionName,
                        function.Runtime,
                        function.Description,
                        function.LastModified,
                        function.MemorySize,
                        function.Timeout))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the full configuration of a single Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the function configuration.</returns>
    [HttpGet("functions/{functionName}")]
    [ProducesResponseType(typeof(LambdaFunctionResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetFunction(string functionName, CancellationToken cancellationToken)
    {
        LogHandlingGet(functionName);
        var result = await _sender.Send(new GetLambdaFunctionQuery(functionName), cancellationToken);
        LogGetHandled(result.IsSuccess);
        return result.Match(
            function => Results.Ok(new LambdaFunctionResponse(
                function.Function.FunctionName,
                function.Function.FunctionArn,
                function.Function.Runtime,
                function.Function.Handler,
                function.Function.Description,
                function.Function.LastModified,
                function.Function.MemorySize,
                function.Function.Timeout,
                function.Function.Role)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the environment variables of a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function to read.</param>
    /// <param name="reveal">Whether to reveal sensitive values, subject to host policy.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the environment variables.</returns>
    [HttpGet("functions/{functionName}/environment")]
    [ProducesResponseType(typeof(LambdaEnvironmentResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetEnvironment(string functionName, [FromQuery] bool reveal, CancellationToken cancellationToken)
    {
        LogHandlingGetEnvironment(functionName, reveal);
        var result = await _sender.Send(new GetLambdaEnvironmentQuery(functionName, reveal), cancellationToken);
        LogGetEnvironmentHandled(result.IsSuccess);
        return result.Match(
            environment => Results.Ok(new LambdaEnvironmentResponse(
                environment.Variables
                    .Select(variable => new LambdaEnvironmentVariableResponse(
                        variable.Name,
                        variable.Value,
                        variable.IsSensitive))
                    .ToList(),
                environment.RevealAllowed)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Replaces the environment variables of a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function to update.</param>
    /// <param name="request">The environment variables to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("functions/{functionName}/environment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateEnvironment(string functionName, [FromBody] LambdaEnvironmentUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateEnvironment(functionName);
        var variables = (request.Variables ?? [])
            .GroupBy(variable => variable.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
        var result = await _sender.Send(new UpdateLambdaEnvironmentCommand(functionName, variables), cancellationToken);
        LogUpdateEnvironmentHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Invokes a Lambda function synchronously and returns the response, status, log tail and duration.
    /// </summary>
    /// <param name="functionName">The name of the function to invoke.</param>
    /// <param name="request">The payload to send to the function.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the invocation result.</returns>
    [HttpPost("functions/{functionName}/invocations")]
    [ProducesResponseType(typeof(LambdaInvocationResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Invoke(string functionName, [FromBody] LambdaInvokeRequest request, CancellationToken cancellationToken)
    {
        LogHandlingInvoke(functionName);
        var result = await _sender.Send(
            new InvokeLambdaFunctionCommand(functionName, request.Payload ?? string.Empty), cancellationToken);
        LogInvokeHandled(result.IsSuccess);
        return result.Match(
            invocation => Results.Ok(new LambdaInvocationResponse(
                invocation.StatusCode,
                invocation.Payload,
                invocation.LogTail,
                invocation.FunctionError,
                invocation.DurationMs)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new Lambda function from a deployment package and configuration.
    /// </summary>
    /// <param name="request">The function to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created function.</returns>
    [HttpPost("functions")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateFunction([FromBody] LambdaFunctionCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreate(request.FunctionName);
        var result = await _sender.Send(
            new CreateLambdaFunctionCommand(
                request.FunctionName,
                request.Runtime,
                request.Handler,
                request.Role,
                request.Description ?? string.Empty,
                request.MemorySize,
                request.Timeout,
                request.ZipFileBase64),
            cancellationToken);
        LogCreateHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created($"/api/services/lambda/functions/{Uri.EscapeDataString(request.FunctionName)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates an existing Lambda function's configuration and, optionally, its code.
    /// </summary>
    /// <param name="functionName">The name of the function to update.</param>
    /// <param name="request">The configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("functions/{functionName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateFunction(string functionName, [FromBody] LambdaFunctionUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdate(functionName);
        var result = await _sender.Send(
            new UpdateLambdaFunctionCommand(
                functionName,
                request.Runtime,
                request.Handler,
                request.Role,
                request.Description ?? string.Empty,
                request.MemorySize,
                request.Timeout,
                request.ZipFileBase64),
            cancellationToken);
        LogUpdateHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("functions/{functionName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteFunction(string functionName, CancellationToken cancellationToken)
    {
        LogHandlingDelete(functionName);
        var result = await _sender.Send(new DeleteLambdaFunctionCommand(functionName), cancellationToken);
        LogDeleteHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the saved test events for a Lambda function together with the starter templates.
    /// </summary>
    /// <param name="functionName">The name of the function whose test events to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the saved events and starter templates.</returns>
    [HttpGet("functions/{functionName}/test-events")]
    [ProducesResponseType(typeof(LambdaTestEventListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListTestEvents(string functionName, CancellationToken cancellationToken)
    {
        LogHandlingListTestEvents(functionName);
        var result = await _sender.Send(new ListLambdaTestEventsQuery(functionName), cancellationToken);
        LogListTestEventsHandled(result.IsSuccess);
        return result.Match(
            events => Results.Ok(new LambdaTestEventListResponse(
                events.Events
                    .Select(testEvent => new LambdaTestEventResponse(testEvent.Name, testEvent.Payload))
                    .ToList(),
                events.Templates
                    .Select(template => new LambdaTestEventResponse(template.Name, template.Payload))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Saves a named test event for a Lambda function, replacing any existing event with the same name.
    /// </summary>
    /// <param name="functionName">The name of the function the event belongs to.</param>
    /// <param name="request">The event to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("functions/{functionName}/test-events")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> SaveTestEvent(string functionName, [FromBody] LambdaTestEventSaveRequest request, CancellationToken cancellationToken)
    {
        LogHandlingSaveTestEvent(functionName, request.Name);
        var result = await _sender.Send(
            new SaveLambdaTestEventCommand(functionName, request.Name, request.Payload ?? "{}"), cancellationToken);
        LogSaveTestEventHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a named test event from a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function the event belongs to.</param>
    /// <param name="name">The name of the test event to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("functions/{functionName}/test-events/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteTestEvent(string functionName, string name, CancellationToken cancellationToken)
    {
        LogHandlingDeleteTestEvent(functionName, name);
        var result = await _sender.Send(new DeleteLambdaTestEventCommand(functionName, name), cancellationToken);
        LogDeleteTestEventHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the event source mappings configured for a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function whose event source mappings to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the event source mappings.</returns>
    [HttpGet("functions/{functionName}/event-source-mappings")]
    [ProducesResponseType(typeof(LambdaEventSourceMappingListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListEventSourceMappings(string functionName, CancellationToken cancellationToken)
    {
        LogHandlingListEventSourceMappings(functionName);
        var result = await _sender.Send(new ListLambdaEventSourceMappingsQuery(functionName), cancellationToken);
        LogListEventSourceMappingsHandled(result.IsSuccess);
        return result.Match(
            mappings => Results.Ok(new LambdaEventSourceMappingListResponse(
                mappings.Mappings
                    .Select(mapping => new LambdaEventSourceMappingResponse(
                        mapping.Uuid,
                        mapping.EventSourceArn,
                        mapping.FunctionArn,
                        mapping.State,
                        mapping.BatchSize,
                        mapping.LastModified))
                    .ToList(),
                mappings.S3Triggers
                    .Select(trigger => new LambdaS3TriggerResponse(trigger.BucketArn))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Enables or disables an event source mapping that triggers a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function the mapping triggers.</param>
    /// <param name="uuid">The unique identifier of the mapping to update.</param>
    /// <param name="request">The desired enabled state.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("functions/{functionName}/event-source-mappings/{uuid}/state")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> SetEventSourceMappingState(string functionName, string uuid, [FromBody] LambdaEventSourceMappingStateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingSetEventSourceMappingState(functionName, uuid, request.Enabled);
        var result = await _sender.Send(
            new SetLambdaEventSourceMappingStateCommand(functionName, uuid, request.Enabled), cancellationToken);
        LogSetEventSourceMappingStateHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reads the most recent CloudWatch log events emitted by a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function whose log events to read.</param>
    /// <param name="limit">The maximum number of log events to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the log events.</returns>
    [HttpGet("functions/{functionName}/logs")]
    [ProducesResponseType(typeof(LambdaLogEventListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListLogEvents(string functionName, [FromQuery] int limit, CancellationToken cancellationToken)
    {
        LogHandlingListLogEvents(functionName, limit);
        var result = await _sender.Send(new ListLambdaLogEventsQuery(functionName, limit), cancellationToken);
        LogListLogEventsHandled(result.IsSuccess);
        return result.Match(
            logs => Results.Ok(new LambdaLogEventListResponse(
                logs.LogGroupName,
                logs.Events
                    .Select(logEvent => new LambdaLogEventResponse(
                        logEvent.Timestamp,
                        logEvent.Message,
                        logEvent.LogStreamName))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Derives invocation monitoring information for a Lambda function from its recent CloudWatch log events.
    /// </summary>
    /// <param name="functionName">The name of the function to analyse.</param>
    /// <param name="limit">The maximum number of log events to analyse.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the derived invocation insights.</returns>
    [HttpGet("functions/{functionName}/invocation-insights")]
    [ProducesResponseType(typeof(LambdaInvocationInsightsResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetInvocationInsights(string functionName, [FromQuery] int limit, CancellationToken cancellationToken)
    {
        LogHandlingGetInvocationInsights(functionName, limit);
        var result = await _sender.Send(new GetLambdaInvocationInsightsQuery(functionName, limit), cancellationToken);
        LogGetInvocationInsightsHandled(result.IsSuccess);
        return result.Match(
            insights => Results.Ok(new LambdaInvocationInsightsResponse(
                insights.LogGroupName,
                new LambdaInvocationMetricsResponse(
                    insights.Insights.Metrics.InvocationCount,
                    insights.Insights.Metrics.ErrorCount,
                    insights.Insights.Metrics.AverageDurationMs,
                    insights.Insights.Metrics.MaxDurationMs),
                insights.Insights.RecentInvocations
                    .Select(invocation => new LambdaRecentInvocationResponse(
                        invocation.RequestId,
                        invocation.Timestamp,
                        invocation.DurationMs,
                        invocation.HasError))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the layer versions attached to a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function whose layers to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the attached layers.</returns>
    [HttpGet("functions/{functionName}/layers")]
    [ProducesResponseType(typeof(LambdaLayerListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListLayers(string functionName, CancellationToken cancellationToken)
    {
        LogHandlingListLayers(functionName);
        var result = await _sender.Send(new ListLambdaLayersQuery(functionName), cancellationToken);
        LogListLayersHandled(result.IsSuccess);
        return result.Match(
            layers => Results.Ok(new LambdaLayerListResponse(
                layers.Layers
                    .Select(layer => new LambdaLayerResponse(
                        layer.Arn,
                        layer.Name,
                        layer.Version))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling Lambda function list request.")]
    private partial void LogHandlingList();

    [LoggerMessage(LogLevel.Trace, "Lambda function list request handled. Success: {Success}")]
    private partial void LogListHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda function get request for '{FunctionName}'.")]
    private partial void LogHandlingGet(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda function get request handled. Success: {Success}")]
    private partial void LogGetHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda environment get request for '{FunctionName}'. Reveal: {Reveal}")]
    private partial void LogHandlingGetEnvironment(string functionName, bool reveal);

    [LoggerMessage(LogLevel.Trace, "Lambda environment get request handled. Success: {Success}")]
    private partial void LogGetEnvironmentHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda environment update request for '{FunctionName}'.")]
    private partial void LogHandlingUpdateEnvironment(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda environment update request handled. Success: {Success}")]
    private partial void LogUpdateEnvironmentHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda invoke request for '{FunctionName}'.")]
    private partial void LogHandlingInvoke(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda invoke request handled. Success: {Success}")]
    private partial void LogInvokeHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda function create request for '{FunctionName}'.")]
    private partial void LogHandlingCreate(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda function create request handled. Success: {Success}")]
    private partial void LogCreateHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda function update request for '{FunctionName}'.")]
    private partial void LogHandlingUpdate(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda function update request handled. Success: {Success}")]
    private partial void LogUpdateHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda function delete request for '{FunctionName}'.")]
    private partial void LogHandlingDelete(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda function delete request handled. Success: {Success}")]
    private partial void LogDeleteHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda test event list request for '{FunctionName}'.")]
    private partial void LogHandlingListTestEvents(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda test event list request handled. Success: {Success}")]
    private partial void LogListTestEventsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda test event save request for '{FunctionName}' named '{Name}'.")]
    private partial void LogHandlingSaveTestEvent(string functionName, string name);

    [LoggerMessage(LogLevel.Trace, "Lambda test event save request handled. Success: {Success}")]
    private partial void LogSaveTestEventHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda test event delete request for '{FunctionName}' named '{Name}'.")]
    private partial void LogHandlingDeleteTestEvent(string functionName, string name);

    [LoggerMessage(LogLevel.Trace, "Lambda test event delete request handled. Success: {Success}")]
    private partial void LogDeleteTestEventHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda event source mapping list request for '{FunctionName}'.")]
    private partial void LogHandlingListEventSourceMappings(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda event source mapping list request handled. Success: {Success}")]
    private partial void LogListEventSourceMappingsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda event source mapping state request for '{FunctionName}' mapping '{Uuid}'. Enabled: {Enabled}")]
    private partial void LogHandlingSetEventSourceMappingState(string functionName, string uuid, bool enabled);

    [LoggerMessage(LogLevel.Trace, "Lambda event source mapping state request handled. Success: {Success}")]
    private partial void LogSetEventSourceMappingStateHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda log event list request for '{FunctionName}'. Limit: {Limit}")]
    private partial void LogHandlingListLogEvents(string functionName, int limit);

    [LoggerMessage(LogLevel.Trace, "Lambda log event list request handled. Success: {Success}")]
    private partial void LogListLogEventsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda invocation insights request for '{FunctionName}'. Limit: {Limit}")]
    private partial void LogHandlingGetInvocationInsights(string functionName, int limit);

    [LoggerMessage(LogLevel.Trace, "Lambda invocation insights request handled. Success: {Success}")]
    private partial void LogGetInvocationInsightsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Lambda layer list request for '{FunctionName}'.")]
    private partial void LogHandlingListLayers(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda layer list request handled. Success: {Success}")]
    private partial void LogListLayersHandled(bool success);
}
