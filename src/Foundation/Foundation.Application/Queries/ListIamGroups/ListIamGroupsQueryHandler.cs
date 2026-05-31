using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListIamGroups;

internal sealed partial class ListIamGroupsQueryHandler : IQueryHandler<ListIamGroupsQuery, ListIamGroupsQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public ListIamGroupsQueryHandler(IIamClient client, ILogger<ListIamGroupsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListIamGroupsQueryResult>> Handle(ListIamGroupsQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var groups = await _client.ListGroupsAsync(cancellationToken);
        LogHandled(groups.IsSuccess);

        if (!groups.IsSuccess)
        {
            Result<ListIamGroupsQueryResult> failure = groups.Error!.Value;
            return failure;
        }

        return new ListIamGroupsQueryResult(groups.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing IAM groups.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "IAM group list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
