using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetUserPool;

internal sealed partial class GetUserPoolQueryHandler : IQueryHandler<GetUserPoolQuery, GetUserPoolQueryResult>
{
    private readonly ICognitoClient _client;
    private readonly ILogger _logger;

    public GetUserPoolQueryHandler(ICognitoClient client, ILogger<GetUserPoolQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetUserPoolQueryResult>> Handle(GetUserPoolQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Id);
        var userPool = await _client.GetUserPoolAsync(request.Id, cancellationToken);
        LogHandled(userPool.IsSuccess);

        if (!userPool.IsSuccess)
        {
            Result<GetUserPoolQueryResult> failure = userPool.Error!.Value;
            return failure;
        }

        return new GetUserPoolQueryResult(userPool.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading Cognito user pool {Id}.")]
    private partial void LogHandling(string id);

    [LoggerMessage(LogLevel.Trace, "Cognito user pool read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
