using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListIamPolicies;

internal sealed partial class ListIamPoliciesQueryHandler : IQueryHandler<ListIamPoliciesQuery, ListIamPoliciesQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public ListIamPoliciesQueryHandler(IIamClient client, ILogger<ListIamPoliciesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListIamPoliciesQueryResult>> Handle(ListIamPoliciesQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.AwsManaged);
        var policies = await _client.ListPoliciesAsync(request.AwsManaged, cancellationToken);
        LogHandled(policies.IsSuccess);

        if (!policies.IsSuccess)
        {
            Result<ListIamPoliciesQueryResult> failure = policies.Error!.Value;
            return failure;
        }

        return new ListIamPoliciesQueryResult(policies.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing IAM policies. AwsManaged: {AwsManaged}")]
    private partial void LogHandling(bool awsManaged);

    [LoggerMessage(LogLevel.Trace, "IAM policy list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
