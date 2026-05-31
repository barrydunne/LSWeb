using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListPolicyVersions;

internal sealed partial class ListPolicyVersionsQueryHandler : IQueryHandler<ListPolicyVersionsQuery, ListPolicyVersionsQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public ListPolicyVersionsQueryHandler(IIamClient client, ILogger<ListPolicyVersionsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListPolicyVersionsQueryResult>> Handle(ListPolicyVersionsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.PolicyArn);
        var versions = await _client.ListPolicyVersionsAsync(request.PolicyArn, cancellationToken);
        LogHandled(versions.IsSuccess);

        if (!versions.IsSuccess)
        {
            Result<ListPolicyVersionsQueryResult> failure = versions.Error!.Value;
            return failure;
        }

        return new ListPolicyVersionsQueryResult(versions.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing versions of IAM policy {PolicyArn}.")]
    private partial void LogHandling(string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM policy version list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
