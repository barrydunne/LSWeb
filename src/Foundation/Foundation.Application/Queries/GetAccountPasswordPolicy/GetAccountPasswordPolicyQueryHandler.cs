using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetAccountPasswordPolicy;

internal sealed partial class GetAccountPasswordPolicyQueryHandler
    : IQueryHandler<GetAccountPasswordPolicyQuery, GetAccountPasswordPolicyQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public GetAccountPasswordPolicyQueryHandler(IIamClient client, ILogger<GetAccountPasswordPolicyQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetAccountPasswordPolicyQueryResult>> Handle(
        GetAccountPasswordPolicyQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var policy = await _client.GetAccountPasswordPolicyAsync(cancellationToken);
        LogHandled(policy.IsSuccess);

        if (!policy.IsSuccess)
        {
            Result<GetAccountPasswordPolicyQueryResult> failure = policy.Error!.Value;
            return failure;
        }

        return new GetAccountPasswordPolicyQueryResult(policy.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting IAM account password policy.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "IAM account password policy retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
