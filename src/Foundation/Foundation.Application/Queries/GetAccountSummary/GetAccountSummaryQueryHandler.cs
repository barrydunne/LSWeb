using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetAccountSummary;

internal sealed partial class GetAccountSummaryQueryHandler : IQueryHandler<GetAccountSummaryQuery, GetAccountSummaryQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public GetAccountSummaryQueryHandler(IIamClient client, ILogger<GetAccountSummaryQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetAccountSummaryQueryResult>> Handle(GetAccountSummaryQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var summary = await _client.GetAccountSummaryAsync(cancellationToken);
        LogHandled(summary.IsSuccess);

        if (!summary.IsSuccess)
        {
            Result<GetAccountSummaryQueryResult> failure = summary.Error!.Value;
            return failure;
        }

        return new GetAccountSummaryQueryResult(summary.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting IAM account summary.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "IAM account summary retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
