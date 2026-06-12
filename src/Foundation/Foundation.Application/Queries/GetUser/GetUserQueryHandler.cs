using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetUser;

internal sealed partial class GetUserQueryHandler : IQueryHandler<GetUserQuery, GetUserQueryResult>
{
    private readonly ICognitoClient _client;
    private readonly ILogger _logger;

    public GetUserQueryHandler(ICognitoClient client, ILogger<GetUserQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetUserQueryResult>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.UserPoolId, request.Username);
        var user = await _client.GetUserAsync(request.UserPoolId, request.Username, cancellationToken);
        LogHandled(user.IsSuccess);

        if (!user.IsSuccess)
        {
            Result<GetUserQueryResult> failure = user.Error!.Value;
            return failure;
        }

        return new GetUserQueryResult(user.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading Cognito user {Username} in user pool {UserPoolId}.")]
    private partial void LogHandling(string userPoolId, string username);

    [LoggerMessage(LogLevel.Trace, "Cognito user read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
