using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateSnsTopic;
using Foundation.Application.Commands.DeleteSnsTopic;
using Foundation.Application.Queries.ListSnsSubscriptions;
using Foundation.Application.Queries.ListSnsTopics;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS SNS: listing the available topics, creating a new topic, and deleting an
/// existing one.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/sns")]
public partial class SnsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SnsController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public SnsController(ISender sender, ILogger<SnsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the SNS topics available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the topic summaries.</returns>
    [HttpGet("topics")]
    [ProducesResponseType(typeof(SnsTopicListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListTopics(CancellationToken cancellationToken)
    {
        LogHandlingListTopics();
        var result = await _sender.Send(new ListSnsTopicsQuery(), cancellationToken);
        LogListTopicsHandled(result.IsSuccess);
        return result.Match(
            topics => Results.Ok(new SnsTopicListResponse(
                topics.Topics
                    .Select(topic => new SnsTopicSummaryResponse(topic.Name, topic.TopicArn))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates an SNS topic with the supplied name.
    /// </summary>
    /// <param name="request">The topic to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created topic.</returns>
    [HttpPost("topics")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateTopic(
        [FromBody] SnsTopicCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateTopic(request.Name);
        var result = await _sender.Send(new CreateSnsTopicCommand(request.Name), cancellationToken);
        LogCreateTopicHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/sns/topics?name={Uri.EscapeDataString(request.Name)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an SNS topic by its Amazon Resource Name. This is a destructive action that cannot be
    /// undone.
    /// </summary>
    /// <param name="arn">The Amazon Resource Name of the topic to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("topics")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteTopic(
        [FromQuery] string arn, CancellationToken cancellationToken)
    {
        LogHandlingDeleteTopic(arn);
        var result = await _sender.Send(new DeleteSnsTopicCommand(arn), cancellationToken);
        LogDeleteTopicHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the subscriptions attached to an SNS topic.
    /// </summary>
    /// <param name="arn">The Amazon Resource Name of the topic to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the subscription summaries.</returns>
    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(SnsSubscriptionListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListSubscriptions(
        [FromQuery] string arn, CancellationToken cancellationToken)
    {
        LogHandlingListSubscriptions(arn);
        var result = await _sender.Send(new ListSnsSubscriptionsQuery(arn), cancellationToken);
        LogListSubscriptionsHandled(result.IsSuccess);
        return result.Match(
            subscriptions => Results.Ok(new SnsSubscriptionListResponse(
                subscriptions.Subscriptions
                    .Select(subscription => new SnsSubscriptionSummaryResponse(
                        subscription.SubscriptionArn,
                        subscription.Protocol,
                        subscription.Endpoint,
                        subscription.Owner))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling SNS topic list request.")]
    private partial void LogHandlingListTopics();

    [LoggerMessage(LogLevel.Trace, "SNS topic list request handled. Success: {Success}")]
    private partial void LogListTopicsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SNS topic create request for {Name}.")]
    private partial void LogHandlingCreateTopic(string name);

    [LoggerMessage(LogLevel.Trace, "SNS topic create request handled. Success: {Success}")]
    private partial void LogCreateTopicHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SNS topic delete request for {Arn}.")]
    private partial void LogHandlingDeleteTopic(string arn);

    [LoggerMessage(LogLevel.Trace, "SNS topic delete request handled. Success: {Success}")]
    private partial void LogDeleteTopicHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SNS subscription list request for {Arn}.")]
    private partial void LogHandlingListSubscriptions(string arn);

    [LoggerMessage(LogLevel.Trace, "SNS subscription list request handled. Success: {Success}")]
    private partial void LogListSubscriptionsHandled(bool success);
}
