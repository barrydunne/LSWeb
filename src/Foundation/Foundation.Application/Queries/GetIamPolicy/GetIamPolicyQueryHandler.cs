using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetIamPolicy;

internal sealed partial class GetIamPolicyQueryHandler : IQueryHandler<GetIamPolicyQuery, GetIamPolicyQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public GetIamPolicyQueryHandler(IIamClient client, ILogger<GetIamPolicyQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetIamPolicyQueryResult>> Handle(GetIamPolicyQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.PolicyArn);
        var policy = await _client.GetPolicyAsync(request.PolicyArn, cancellationToken);
        LogHandled(policy.IsSuccess);

        if (!policy.IsSuccess)
        {
            Result<GetIamPolicyQueryResult> failure = policy.Error!.Value;
            return failure;
        }

        return new GetIamPolicyQueryResult(policy.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting IAM policy {PolicyArn}.")]
    private partial void LogHandling(string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM policy retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
