using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListStackResources;

internal sealed partial class ListStackResourcesQueryHandler
    : IQueryHandler<ListStackResourcesQuery, ListStackResourcesQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public ListStackResourcesQueryHandler(
        ICloudFormationClient client, ILogger<ListStackResourcesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListStackResourcesQueryResult>> Handle(
        ListStackResourcesQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.StackName);
        var resources = await _client.ListStackResourcesAsync(request.StackName, cancellationToken);
        LogHandled(resources.IsSuccess);

        if (!resources.IsSuccess)
        {
            Result<ListStackResourcesQueryResult> failure = resources.Error!.Value;
            return failure;
        }

        return new ListStackResourcesQueryResult(resources.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing CloudFormation stack resources {StackName}.")]
    private partial void LogHandling(string stackName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack resources list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
