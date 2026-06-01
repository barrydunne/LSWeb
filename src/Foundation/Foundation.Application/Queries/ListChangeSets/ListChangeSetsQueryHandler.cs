using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListChangeSets;

internal sealed partial class ListChangeSetsQueryHandler
    : IQueryHandler<ListChangeSetsQuery, ListChangeSetsQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public ListChangeSetsQueryHandler(
        ICloudFormationClient client, ILogger<ListChangeSetsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListChangeSetsQueryResult>> Handle(
        ListChangeSetsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.StackName);
        var changeSets = await _client.ListChangeSetsAsync(request.StackName, cancellationToken);
        LogHandled(changeSets.IsSuccess);

        if (!changeSets.IsSuccess)
        {
            Result<ListChangeSetsQueryResult> failure = changeSets.Error!.Value;
            return failure;
        }

        return new ListChangeSetsQueryResult(changeSets.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing CloudFormation change sets {StackName}.")]
    private partial void LogHandling(string stackName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation change sets list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
