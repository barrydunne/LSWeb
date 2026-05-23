using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateSqsQueue;
using Foundation.Application.Commands.DeleteSqsMessage;
using Foundation.Application.Commands.DeleteSqsQueue;
using Foundation.Application.Commands.PurgeSqsQueue;
using Foundation.Application.Commands.RedriveSqsMessages;
using Foundation.Application.Commands.SendSqsMessage;
using Foundation.Application.Commands.SetSqsQueueAttributes;
using Foundation.Application.Queries.GetSqsQueueAttributes;
using Foundation.Application.Queries.GetSqsQueueRedrive;
using Foundation.Application.Queries.ListSqsMessages;
using Foundation.Application.Queries.ListSqsQueues;
using Foundation.Application.Queries.ListSqsSubscriptions;
using Foundation.Application.Sqs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS SQS queues: listing the available queues with their approximate message
/// counts.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/sqs")]
public partial class SqsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqsController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public SqsController(ISender sender, ILogger<SqsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the SQS queues available on the configured backend, including their approximate message counts.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the queue summaries.</returns>
    [HttpGet("queues")]
    [ProducesResponseType(typeof(SqsQueueListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListQueues(CancellationToken cancellationToken)
    {
        LogHandlingList();
        var result = await _sender.Send(new ListSqsQueuesQuery(), cancellationToken);
        LogListHandled(result.IsSuccess);
        return result.Match(
            queues => Results.Ok(new SqsQueueListResponse(
                queues.Queues
                    .Select(queue => new SqsQueueResponse(
                        queue.Name,
                        queue.Url,
                        queue.ApproximateMessageCount,
                        queue.ApproximateInFlightCount,
                        queue.ApproximateDelayedCount))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new SQS queue, either a standard queue or a FIFO queue.
    /// </summary>
    /// <param name="request">The queue to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created queue.</returns>
    [HttpPost("queues")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateQueue([FromBody] SqsQueueCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreate(request.QueueName, request.FifoQueue);
        var result = await _sender.Send(
            new CreateSqsQueueCommand(request.QueueName, request.FifoQueue), cancellationToken);
        LogCreateHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created($"/api/services/sqs/queues/{Uri.EscapeDataString(request.QueueName)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an SQS queue and all of its messages. This is a destructive action that cannot be undone.
    /// </summary>
    /// <param name="queueName">The name of the queue to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("queues/{queueName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteQueue(string queueName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteQueue(queueName);
        var result = await _sender.Send(new DeleteSqsQueueCommand(queueName), cancellationToken);
        LogDeleteQueueHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Polls a queue for messages, distinguishing peek (visibility-preserving) from consume.
    /// </summary>
    /// <param name="queueName">The name of the queue to read from.</param>
    /// <param name="mode">Either <c>peek</c> (the default, preserves visibility) or <c>consume</c>.</param>
    /// <param name="maxMessages">The maximum number of messages to return; clamped to 1-10.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the messages.</returns>
    [HttpGet("queues/{queueName}/messages")]
    [ProducesResponseType(typeof(SqsMessageListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> PollMessages(
        string queueName,
        [FromQuery] string? mode,
        [FromQuery] int maxMessages,
        CancellationToken cancellationToken)
    {
        var pollMode = string.Equals(mode, "consume", StringComparison.OrdinalIgnoreCase)
            ? SqsPollMode.Consume
            : SqsPollMode.Peek;
        var requested = maxMessages <= 0 ? 10 : maxMessages;

        LogHandlingPoll(queueName, pollMode);
        var result = await _sender.Send(
            new ListSqsMessagesQuery(queueName, pollMode, requested), cancellationToken);
        LogPollHandled(result.IsSuccess);
        return result.Match(
            messages => Results.Ok(new SqsMessageListResponse(
                messages.Messages
                    .Select(message => new SqsMessageResponse(
                        message.MessageId,
                        message.ReceiptHandle,
                        message.Body,
                        message.Attributes,
                        message.MessageAttributes))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a single message from a queue using its receipt handle.
    /// </summary>
    /// <param name="queueName">The name of the queue the message was received from.</param>
    /// <param name="receiptHandle">The receipt handle identifying the message to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("queues/{queueName}/messages")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteMessage(
        string queueName, [FromQuery] string receiptHandle, CancellationToken cancellationToken)
    {
        LogHandlingDelete(queueName);
        var result = await _sender.Send(
            new DeleteSqsMessageCommand(queueName, receiptHandle), cancellationToken);
        LogDeleteHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Purges all messages from a queue. This is a destructive action that cannot be undone.
    /// </summary>
    /// <param name="queueName">The name of the queue to purge.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("queues/{queueName}/purge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> PurgeQueue(string queueName, CancellationToken cancellationToken)
    {
        LogHandlingPurge(queueName);
        var result = await _sender.Send(new PurgeSqsQueueCommand(queueName), cancellationToken);
        LogPurgeHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Sends a message to a queue, optionally with custom attributes and, for FIFO queues, a message
    /// group id and deduplication id.
    /// </summary>
    /// <param name="queueName">The name of the queue to send the message to.</param>
    /// <param name="request">The message to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 202 result on success.</returns>
    [HttpPost("queues/{queueName}/messages")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IResult> SendMessage(
        string queueName, [FromBody] SqsSendMessageRequest request, CancellationToken cancellationToken)
    {
        LogHandlingSend(queueName);
        var result = await _sender.Send(
            new SendSqsMessageCommand(
                queueName,
                request.Body,
                request.MessageAttributes ?? new Dictionary<string, string>(),
                request.MessageGroupId,
                request.MessageDeduplicationId),
            cancellationToken);
        LogSendHandled(result.IsSuccess);
        return result.Match(
            () => Results.Accepted(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the SNS topics that publish to a queue, detected from the queue's access policy, so the
    /// relationship can be shown as cross-resource links.
    /// </summary>
    /// <param name="queueName">The name of the queue to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the subscriptions.</returns>
    [HttpGet("queues/{queueName}/subscriptions")]
    [ProducesResponseType(typeof(SqsSubscriptionListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListSubscriptions(string queueName, CancellationToken cancellationToken)
    {
        LogHandlingSubscriptions(queueName);
        var result = await _sender.Send(new ListSqsSubscriptionsQuery(queueName), cancellationToken);
        LogSubscriptionsHandled(result.IsSuccess);
        return result.Match(
            subscriptions => Results.Ok(new SqsSubscriptionListResponse(
                subscriptions.Subscriptions
                    .Select(subscription => new SqsSubscriptionResponse(
                        subscription.TopicArn,
                        subscription.TopicName))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reads the configurable and informational attributes of a queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the queue attributes.</returns>
    [HttpGet("queues/{queueName}/attributes")]
    [ProducesResponseType(typeof(SqsQueueAttributesResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetAttributes(string queueName, CancellationToken cancellationToken)
    {
        LogHandlingGetAttributes(queueName);
        var result = await _sender.Send(new GetSqsQueueAttributesQuery(queueName), cancellationToken);
        LogGetAttributesHandled(result.IsSuccess);
        return result.Match(
            attributes => Results.Ok(new SqsQueueAttributesResponse(
                attributes.Attributes.VisibilityTimeoutSeconds,
                attributes.Attributes.MessageRetentionPeriodSeconds,
                attributes.Attributes.DelaySeconds,
                attributes.Attributes.ReceiveMessageWaitTimeSeconds,
                attributes.Attributes.MaximumMessageSizeBytes,
                attributes.Attributes.QueueArn,
                attributes.Attributes.FifoQueue)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates the editable attributes of a queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to update.</param>
    /// <param name="request">The attribute values to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("queues/{queueName}/attributes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> SetAttributes(
        string queueName, [FromBody] SqsQueueAttributesUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingSetAttributes(queueName);
        var result = await _sender.Send(
            new SetSqsQueueAttributesCommand(
                queueName,
                request.VisibilityTimeoutSeconds,
                request.MessageRetentionPeriodSeconds,
                request.DelaySeconds,
                request.ReceiveMessageWaitTimeSeconds),
            cancellationToken);
        LogSetAttributesHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reads the dead-letter queue relationships of a queue: the dead-letter queue it feeds and the
    /// source queues permitted to use it as their dead-letter queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the redrive relationships.</returns>
    [HttpGet("queues/{queueName}/redrive")]
    [ProducesResponseType(typeof(SqsRedriveResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetRedrive(string queueName, CancellationToken cancellationToken)
    {
        LogHandlingGetRedrive(queueName);
        var result = await _sender.Send(new GetSqsQueueRedriveQuery(queueName), cancellationToken);
        LogGetRedriveHandled(result.IsSuccess);
        return result.Match(
            redrive => Results.Ok(new SqsRedriveResponse(
                redrive.Redrive.DeadLetterTarget is null
                    ? null
                    : new SqsRedriveTargetResponse(
                        redrive.Redrive.DeadLetterTarget.QueueArn,
                        redrive.Redrive.DeadLetterTarget.QueueName,
                        redrive.Redrive.DeadLetterTarget.MaxReceiveCount),
                redrive.Redrive.Sources
                    .Select(source => new SqsRedriveSourceResponse(source.QueueArn, source.QueueName))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Starts moving messages from a dead-letter queue back to their source queues.
    /// </summary>
    /// <param name="queueName">The name of the dead-letter queue to redrive messages from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 202 result on success.</returns>
    [HttpPost("queues/{queueName}/redrive")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IResult> Redrive(string queueName, CancellationToken cancellationToken)
    {
        LogHandlingRedrive(queueName);
        var result = await _sender.Send(new RedriveSqsMessagesCommand(queueName), cancellationToken);
        LogRedriveHandled(result.IsSuccess);
        return result.Match(
            () => Results.Accepted(),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling SQS queue list request.")]
    private partial void LogHandlingList();

    [LoggerMessage(LogLevel.Trace, "SQS queue list request handled. Success: {Success}")]
    private partial void LogListHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS create queue request for {QueueName}. FIFO: {Fifo}")]
    private partial void LogHandlingCreate(string queueName, bool fifo);

    [LoggerMessage(LogLevel.Trace, "SQS create queue request handled. Success: {Success}")]
    private partial void LogCreateHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS delete queue request for {QueueName}.")]
    private partial void LogHandlingDeleteQueue(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS delete queue request handled. Success: {Success}")]
    private partial void LogDeleteQueueHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS poll request for {QueueName} in {Mode} mode.")]
    private partial void LogHandlingPoll(string queueName, SqsPollMode mode);

    [LoggerMessage(LogLevel.Trace, "SQS poll request handled. Success: {Success}")]
    private partial void LogPollHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS delete message request for {QueueName}.")]
    private partial void LogHandlingDelete(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS delete message request handled. Success: {Success}")]
    private partial void LogDeleteHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS purge request for {QueueName}.")]
    private partial void LogHandlingPurge(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS purge request handled. Success: {Success}")]
    private partial void LogPurgeHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS send message request for {QueueName}.")]
    private partial void LogHandlingSend(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS send message request handled. Success: {Success}")]
    private partial void LogSendHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS subscription list request for {QueueName}.")]
    private partial void LogHandlingSubscriptions(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS subscription list request handled. Success: {Success}")]
    private partial void LogSubscriptionsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS get attributes request for {QueueName}.")]
    private partial void LogHandlingGetAttributes(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS get attributes request handled. Success: {Success}")]
    private partial void LogGetAttributesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS set attributes request for {QueueName}.")]
    private partial void LogHandlingSetAttributes(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS set attributes request handled. Success: {Success}")]
    private partial void LogSetAttributesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS get redrive request for {QueueName}.")]
    private partial void LogHandlingGetRedrive(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS get redrive request handled. Success: {Success}")]
    private partial void LogGetRedriveHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SQS redrive request for {QueueName}.")]
    private partial void LogHandlingRedrive(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS redrive request handled. Success: {Success}")]
    private partial void LogRedriveHandled(bool success);
}
