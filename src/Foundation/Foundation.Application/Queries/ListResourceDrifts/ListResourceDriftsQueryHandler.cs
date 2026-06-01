using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListResourceDrifts;

internal sealed partial class ListResourceDriftsQueryHandler
    : IQueryHandler<ListResourceDriftsQuery, ListResourceDriftsQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public ListResourceDriftsQueryHandler(
        ICloudFormationClient client, ILogger<ListResourceDriftsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListResourceDriftsQueryResult>> Handle(
        ListResourceDriftsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.StackName);
        var drifts = await _client.DescribeStackResourceDriftsAsync(
            request.StackName, cancellationToken);
        LogHandled(drifts.IsSuccess);

        if (!drifts.IsSuccess)
        {
            Result<ListResourceDriftsQueryResult> failure = drifts.Error!.Value;
            return failure;
        }

        return new ListResourceDriftsQueryResult(drifts.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing CloudFormation resource drifts on {StackName}.")]
    private partial void LogHandling(string stackName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation resource drifts handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
