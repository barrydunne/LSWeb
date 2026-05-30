using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sns;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetSnsSubscriptionFilterPolicy;

internal sealed partial class GetSnsSubscriptionFilterPolicyQueryHandler
    : IQueryHandler<GetSnsSubscriptionFilterPolicyQuery, GetSnsSubscriptionFilterPolicyQueryResult>
{
    private readonly ISnsClient _client;
    private readonly ILogger _logger;

    public GetSnsSubscriptionFilterPolicyQueryHandler(
        ISnsClient client, ILogger<GetSnsSubscriptionFilterPolicyQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetSnsSubscriptionFilterPolicyQueryResult>> Handle(
        GetSnsSubscriptionFilterPolicyQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.SubscriptionArn);
        var filterPolicy = await _client.GetSubscriptionFilterPolicyAsync(
            request.SubscriptionArn, cancellationToken);
        LogHandled(filterPolicy.IsSuccess);

        if (!filterPolicy.IsSuccess)
        {
            Result<GetSnsSubscriptionFilterPolicyQueryResult> failure = filterPolicy.Error!.Value;
            return failure;
        }

        return new GetSnsSubscriptionFilterPolicyQueryResult(filterPolicy.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting SNS filter policy for subscription {SubscriptionArn}.")]
    private partial void LogHandling(string subscriptionArn);

    [LoggerMessage(LogLevel.Trace, "SNS filter policy get handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
