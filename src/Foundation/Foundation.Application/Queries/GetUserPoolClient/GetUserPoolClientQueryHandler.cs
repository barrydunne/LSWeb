using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetUserPoolClient;

internal sealed partial class GetUserPoolClientQueryHandler : IQueryHandler<GetUserPoolClientQuery, GetUserPoolClientQueryResult>
{
    private readonly ICognitoClient _client;
    private readonly ILogger _logger;

    public GetUserPoolClientQueryHandler(ICognitoClient client, ILogger<GetUserPoolClientQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetUserPoolClientQueryResult>> Handle(GetUserPoolClientQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.UserPoolId, request.ClientId);
        var appClient = await _client.GetUserPoolClientAsync(request.UserPoolId, request.ClientId, cancellationToken);
        LogHandled(appClient.IsSuccess);

        if (!appClient.IsSuccess)
        {
            Result<GetUserPoolClientQueryResult> failure = appClient.Error!.Value;
            return failure;
        }

        return new GetUserPoolClientQueryResult(appClient.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading Cognito app client {ClientId} in user pool {UserPoolId}.")]
    private partial void LogHandling(string userPoolId, string clientId);

    [LoggerMessage(LogLevel.Trace, "Cognito app client read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
