using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListUserPools;

internal sealed partial class ListUserPoolsQueryHandler : IQueryHandler<ListUserPoolsQuery, ListUserPoolsQueryResult>
{
    private readonly ICognitoClient _client;
    private readonly ILogger _logger;

    public ListUserPoolsQueryHandler(ICognitoClient client, ILogger<ListUserPoolsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListUserPoolsQueryResult>> Handle(ListUserPoolsQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var userPools = await _client.ListUserPoolsAsync(cancellationToken);
        LogHandled(userPools.IsSuccess);

        if (!userPools.IsSuccess)
        {
            Result<ListUserPoolsQueryResult> failure = userPools.Error!.Value;
            return failure;
        }

        return new ListUserPoolsQueryResult(userPools.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Cognito user pools.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Cognito user pool listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
