using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.RequestCognitoToken;

internal sealed partial class RequestCognitoTokenQueryHandler : IQueryHandler<RequestCognitoTokenQuery, RequestCognitoTokenQueryResult>
{
    private readonly ICognitoClient _client;
    private readonly ILogger _logger;

    public RequestCognitoTokenQueryHandler(ICognitoClient client, ILogger<RequestCognitoTokenQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<RequestCognitoTokenQueryResult>> Handle(RequestCognitoTokenQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.ClientId, request.Username);
        var token = await _client.RequestTokenAsync(
            request.UserPoolId, request.ClientId, request.Username, request.Password, cancellationToken);
        LogHandled(token.IsSuccess);

        if (!token.IsSuccess)
        {
            Result<RequestCognitoTokenQueryResult> failure = token.Error!.Value;
            return failure;
        }

        return new RequestCognitoTokenQueryResult(token.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Requesting Cognito token for client {ClientId} and user {Username}.")]
    private partial void LogHandling(string clientId, string username);

    [LoggerMessage(LogLevel.Trace, "Cognito token request handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
