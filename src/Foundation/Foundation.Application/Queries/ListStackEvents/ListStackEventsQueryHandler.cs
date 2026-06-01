using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListStackEvents;

internal sealed partial class ListStackEventsQueryHandler
    : IQueryHandler<ListStackEventsQuery, ListStackEventsQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public ListStackEventsQueryHandler(
        ICloudFormationClient client, ILogger<ListStackEventsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListStackEventsQueryResult>> Handle(
        ListStackEventsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.StackName);
        var events = await _client.DescribeStackEventsAsync(request.StackName, cancellationToken);
        LogHandled(events.IsSuccess);

        if (!events.IsSuccess)
        {
            Result<ListStackEventsQueryResult> failure = events.Error!.Value;
            return failure;
        }

        return new ListStackEventsQueryResult(events.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing CloudFormation stack events {StackName}.")]
    private partial void LogHandling(string stackName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack events list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
