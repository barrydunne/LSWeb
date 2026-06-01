using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListStacks;

internal sealed partial class ListStacksQueryHandler
    : IQueryHandler<ListStacksQuery, ListStacksQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public ListStacksQueryHandler(
        ICloudFormationClient client, ILogger<ListStacksQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListStacksQueryResult>> Handle(
        ListStacksQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var stacks = await _client.ListStacksAsync(cancellationToken);
        LogHandled(stacks.IsSuccess);

        if (!stacks.IsSuccess)
        {
            Result<ListStacksQueryResult> failure = stacks.Error!.Value;
            return failure;
        }

        return new ListStacksQueryResult(stacks.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing CloudFormation stacks.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
