using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetIamUser;

internal sealed partial class GetIamUserQueryHandler : IQueryHandler<GetIamUserQuery, GetIamUserQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public GetIamUserQueryHandler(IIamClient client, ILogger<GetIamUserQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetIamUserQueryResult>> Handle(GetIamUserQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.UserName);
        var user = await _client.GetUserAsync(request.UserName, cancellationToken);
        LogHandled(user.IsSuccess);

        if (!user.IsSuccess)
        {
            Result<GetIamUserQueryResult> failure = user.Error!.Value;
            return failure;
        }

        return new GetIamUserQueryResult(user.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting IAM user {UserName}.")]
    private partial void LogHandling(string userName);

    [LoggerMessage(LogLevel.Trace, "IAM user retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
