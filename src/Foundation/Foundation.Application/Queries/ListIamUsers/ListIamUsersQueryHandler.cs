using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListIamUsers;

internal sealed partial class ListIamUsersQueryHandler : IQueryHandler<ListIamUsersQuery, ListIamUsersQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public ListIamUsersQueryHandler(IIamClient client, ILogger<ListIamUsersQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListIamUsersQueryResult>> Handle(ListIamUsersQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var users = await _client.ListUsersAsync(cancellationToken);
        LogHandled(users.IsSuccess);

        if (!users.IsSuccess)
        {
            Result<ListIamUsersQueryResult> failure = users.Error!.Value;
            return failure;
        }

        return new ListIamUsersQueryResult(users.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing IAM users.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "IAM user list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
