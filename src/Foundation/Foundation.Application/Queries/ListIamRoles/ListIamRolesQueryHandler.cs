using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListIamRoles;

internal sealed partial class ListIamRolesQueryHandler : IQueryHandler<ListIamRolesQuery, ListIamRolesQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public ListIamRolesQueryHandler(IIamClient client, ILogger<ListIamRolesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListIamRolesQueryResult>> Handle(ListIamRolesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var roles = await _client.ListRolesAsync(cancellationToken);
        LogHandled(roles.IsSuccess);

        if (!roles.IsSuccess)
        {
            Result<ListIamRolesQueryResult> failure = roles.Error!.Value;
            return failure;
        }

        return new ListIamRolesQueryResult(roles.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing IAM roles.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "IAM role list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
