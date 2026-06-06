using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListUserPoolClients;

internal sealed partial class ListUserPoolClientsQueryHandler : IQueryHandler<ListUserPoolClientsQuery, ListUserPoolClientsQueryResult>
{
    private readonly ICognitoClient _client;
    private readonly ILogger _logger;

    public ListUserPoolClientsQueryHandler(ICognitoClient client, ILogger<ListUserPoolClientsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListUserPoolClientsQueryResult>> Handle(ListUserPoolClientsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.UserPoolId);
        var clients = await _client.ListUserPoolClientsAsync(request.UserPoolId, cancellationToken);
        LogHandled(clients.IsSuccess);

        if (!clients.IsSuccess)
        {
            Result<ListUserPoolClientsQueryResult> failure = clients.Error!.Value;
            return failure;
        }

        return new ListUserPoolClientsQueryResult(clients.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Cognito app clients for user pool {UserPoolId}.")]
    private partial void LogHandling(string userPoolId);

    [LoggerMessage(LogLevel.Trace, "Cognito app client listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
