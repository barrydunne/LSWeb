using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListUsers;

internal sealed partial class ListUsersQueryHandler : IQueryHandler<ListUsersQuery, ListUsersQueryResult>
{
    private readonly ICognitoClient _client;
    private readonly ILogger _logger;

    public ListUsersQueryHandler(ICognitoClient client, ILogger<ListUsersQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListUsersQueryResult>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.UserPoolId);
        var users = await _client.ListUsersAsync(request.UserPoolId, cancellationToken);
        LogHandled(users.IsSuccess);

        if (!users.IsSuccess)
        {
            Result<ListUsersQueryResult> failure = users.Error!.Value;
            return failure;
        }

        return new ListUsersQueryResult(users.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Cognito users for user pool {UserPoolId}.")]
    private partial void LogHandling(string userPoolId);

    [LoggerMessage(LogLevel.Trace, "Cognito user listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
