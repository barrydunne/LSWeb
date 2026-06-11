using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateLogGroup;
using Foundation.Application.Commands.CreateLogStream;
using Foundation.Application.Commands.DeleteLogGroup;
using Foundation.Application.Commands.DeleteLogStream;
using Foundation.Application.Queries.FilterLogEvents;
using Foundation.Application.Queries.GetLogEvents;
using Foundation.Application.Queries.ListLogGroups;
using Foundation.Application.Queries.ListLogStreams;
using Foundation.Application.Queries.RunLogInsights;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS CloudWatch Logs: browsing log groups, the streams within a group, and the
/// events within a stream.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/cloudwatch-logs")]
public partial class CloudWatchLogsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudWatchLogsController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public CloudWatchLogsController(ISender sender, ILogger<CloudWatchLogsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the CloudWatch log groups available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the log group summaries.</returns>
    [HttpGet("groups")]
    [ProducesResponseType(typeof(LogGroupListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListGroups(CancellationToken cancellationToken)
    {
        LogHandlingListGroups();
        var result = await _sender.Send(new ListLogGroupsQuery(), cancellationToken);
        LogListGroupsHandled(result.IsSuccess);
        return result.Match(
            groups => Results.Ok(new LogGroupListResponse(
                groups.LogGroups
                    .Select(group => new LogGroupResponse(
                        group.Name,
                        group.Arn,
                        group.StoredBytes,
                        group.RetentionInDays,
                        group.CreatedAt))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the log streams within a log group, most recently active first.
    /// </summary>
    /// <param name="logGroupName">The name of the log group to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the log stream summaries.</returns>
    [HttpGet("streams")]
    [ProducesResponseType(typeof(LogStreamListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListStreams(
        [FromQuery] string logGroupName, CancellationToken cancellationToken)
    {
        LogHandlingListStreams(logGroupName);
        var result = await _sender.Send(new ListLogStreamsQuery(logGroupName), cancellationToken);
        LogListStreamsHandled(result.IsSuccess);
        return result.Match(
            streams => Results.Ok(new LogStreamListResponse(
                streams.LogStreams
                    .Select(stream => new LogStreamResponse(
                        stream.Name,
                        stream.LastEventTimestamp))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reads the most recent events from a log stream.
    /// </summary>
    /// <param name="logGroupName">The name of the log group the stream belongs to.</param>
    /// <param name="logStreamName">The name of the log stream to read from.</param>
    /// <param name="limit">The maximum number of events to return; clamped to 1-1000.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the log events.</returns>
    [HttpGet("events")]
    [ProducesResponseType(typeof(LogEventListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetEvents(
        [FromQuery] string logGroupName,
        [FromQuery] string logStreamName,
        [FromQuery] int limit,
        CancellationToken cancellationToken)
    {
        var requested = limit <= 0 ? 100 : limit;

        LogHandlingGetEvents(logGroupName, logStreamName);
        var result = await _sender.Send(
            new GetLogEventsQuery(logGroupName, logStreamName, requested), cancellationToken);
        LogGetEventsHandled(result.IsSuccess);
        return result.Match(
            events => Results.Ok(new LogEventListResponse(
                events.Events
                    .Select(logEvent => new LogEventResponse(logEvent.Timestamp, logEvent.Message))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Searches the events across every stream in a log group, optionally constrained by a filter
    /// pattern and a start time. Powers filtered search and live tail.
    /// </summary>
    /// <param name="logGroupName">The name of the log group to search.</param>
    /// <param name="filterPattern">The CloudWatch Logs filter pattern, or empty for no filter.</param>
    /// <param name="startTime">Only return events at or after this Unix-millisecond time, or zero for no lower bound.</param>
    /// <param name="limit">The maximum number of events to return; clamped to 1-1000.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the matching log events.</returns>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(LogEventListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> FilterEvents(
        [FromQuery] string logGroupName,
        [FromQuery] string? filterPattern,
        [FromQuery] long? startTime,
        [FromQuery] int limit,
        CancellationToken cancellationToken)
    {
        var requested = limit <= 0 ? 100 : limit;
        var start = startTime is null or <= 0
            ? (DateTimeOffset?)null
            : DateTimeOffset.FromUnixTimeMilliseconds(startTime.Value);

        LogHandlingFilter(logGroupName);
        var result = await _sender.Send(
            new FilterLogEventsQuery(logGroupName, filterPattern, start, requested), cancellationToken);
        LogFilterHandled(result.IsSuccess);
        return result.Match(
            events => Results.Ok(new LogEventListResponse(
                events.Events
                    .Select(logEvent => new LogEventResponse(logEvent.Timestamp, logEvent.Message))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new, empty CloudWatch log group.
    /// </summary>
    /// <param name="request">The log group to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created log group.</returns>
    [HttpPost("groups")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateGroup(
        [FromBody] LogGroupCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreate(request.LogGroupName);
        var result = await _sender.Send(new CreateLogGroupCommand(request.LogGroupName), cancellationToken);
        LogCreateHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/cloudwatch-logs/groups?logGroupName={Uri.EscapeDataString(request.LogGroupName)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a CloudWatch log group and all of the streams and events it contains. This is a
    /// destructive action that cannot be undone.
    /// </summary>
    /// <param name="logGroupName">The name of the log group to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("groups")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteGroup(
        [FromQuery] string logGroupName, CancellationToken cancellationToken)
    {
        LogHandlingDelete(logGroupName);
        var result = await _sender.Send(new DeleteLogGroupCommand(logGroupName), cancellationToken);
        LogDeleteHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new, empty log stream within an existing CloudWatch log group.
    /// </summary>
    /// <param name="request">The log stream to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created log stream.</returns>
    [HttpPost("streams")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateStream(
        [FromBody] LogStreamCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateStream(request.LogGroupName, request.LogStreamName);
        var result = await _sender.Send(
            new CreateLogStreamCommand(request.LogGroupName, request.LogStreamName), cancellationToken);
        LogCreateStreamHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                "/api/services/cloudwatch-logs/streams?logGroupName=" +
                $"{Uri.EscapeDataString(request.LogGroupName)}&logStreamName={Uri.EscapeDataString(request.LogStreamName)}",
                null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a CloudWatch log stream and all of the events it contains. This is a destructive
    /// action that cannot be undone.
    /// </summary>
    /// <param name="logGroupName">The name of the log group the stream belongs to.</param>
    /// <param name="logStreamName">The name of the log stream to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("streams")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteStream(
        [FromQuery] string logGroupName,
        [FromQuery] string logStreamName,
        CancellationToken cancellationToken)
    {
        LogHandlingDeleteStream(logGroupName, logStreamName);
        var result = await _sender.Send(
            new DeleteLogStreamCommand(logGroupName, logStreamName), cancellationToken);
        LogDeleteStreamHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Runs a CloudWatch Logs Insights query against a log group over a time range and waits for the
    /// matching rows.
    /// </summary>
    /// <param name="request">The Insights query to run.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the query outcome and rows.</returns>
    [HttpPost("insights/query")]
    [ProducesResponseType(typeof(LogInsightsQueryResponse), StatusCodes.Status200OK)]
    public async Task<IResult> RunInsightsQuery(
        [FromBody] LogInsightsQueryRequest request, CancellationToken cancellationToken)
    {
        var requested = request.Limit <= 0 ? 1000 : request.Limit;

        LogHandlingInsights(request.LogGroupName);
        var result = await _sender.Send(
            new RunLogInsightsQuery(
                request.LogGroupName,
                request.QueryString,
                request.StartTime,
                request.EndTime,
                requested),
            cancellationToken);
        LogInsightsHandled(result.IsSuccess);
        return result.Match(
            query => Results.Ok(new LogInsightsQueryResponse(
                query.Result.Status,
                query.Result.Rows
                    .Select(row => new LogInsightsRowResponse(
                        row.Fields
                            .Select(field => new LogInsightsFieldResponse(field.Field, field.Value))
                            .ToList()))
                    .ToList(),
                query.Result.RecordsMatched,
                query.Result.RecordsScanned)),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling CloudWatch log group list request.")]
    private partial void LogHandlingListGroups();

    [LoggerMessage(LogLevel.Trace, "CloudWatch log group list request handled. Success: {Success}")]
    private partial void LogListGroupsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudWatch log stream list request for {LogGroupName}.")]
    private partial void LogHandlingListStreams(string logGroupName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log stream list request handled. Success: {Success}")]
    private partial void LogListStreamsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudWatch log event request for {LogGroupName}/{LogStreamName}.")]
    private partial void LogHandlingGetEvents(string logGroupName, string logStreamName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log event request handled. Success: {Success}")]
    private partial void LogGetEventsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudWatch log event filter request for {LogGroupName}.")]
    private partial void LogHandlingFilter(string logGroupName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log event filter request handled. Success: {Success}")]
    private partial void LogFilterHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudWatch log group create request for {LogGroupName}.")]
    private partial void LogHandlingCreate(string logGroupName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log group create request handled. Success: {Success}")]
    private partial void LogCreateHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudWatch log group delete request for {LogGroupName}.")]
    private partial void LogHandlingDelete(string logGroupName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log group delete request handled. Success: {Success}")]
    private partial void LogDeleteHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudWatch log stream create request for {LogGroupName}/{LogStreamName}.")]
    private partial void LogHandlingCreateStream(string logGroupName, string logStreamName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log stream create request handled. Success: {Success}")]
    private partial void LogCreateStreamHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudWatch log stream delete request for {LogGroupName}/{LogStreamName}.")]
    private partial void LogHandlingDeleteStream(string logGroupName, string logStreamName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log stream delete request handled. Success: {Success}")]
    private partial void LogDeleteStreamHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudWatch Logs Insights query request for {LogGroupName}.")]
    private partial void LogHandlingInsights(string logGroupName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch Logs Insights query request handled. Success: {Success}")]
    private partial void LogInsightsHandled(bool success);
}
