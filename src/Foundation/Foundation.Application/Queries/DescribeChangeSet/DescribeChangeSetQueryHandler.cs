using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.DescribeChangeSet;

internal sealed partial class DescribeChangeSetQueryHandler
    : IQueryHandler<DescribeChangeSetQuery, DescribeChangeSetQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public DescribeChangeSetQueryHandler(
        ICloudFormationClient client, ILogger<DescribeChangeSetQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<DescribeChangeSetQueryResult>> Handle(
        DescribeChangeSetQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.StackName, request.ChangeSetName);
        var changeSet = await _client.DescribeChangeSetAsync(
            request.StackName, request.ChangeSetName, cancellationToken);
        LogHandled(changeSet.IsSuccess);

        if (!changeSet.IsSuccess)
        {
            Result<DescribeChangeSetQueryResult> failure = changeSet.Error!.Value;
            return failure;
        }

        return new DescribeChangeSetQueryResult(changeSet.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Describing CloudFormation change set {ChangeSetName} on {StackName}.")]
    private partial void LogHandling(string stackName, string changeSetName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation change set describe handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
