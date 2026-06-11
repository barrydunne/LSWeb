using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.RunLogInsights;

internal sealed partial class RunLogInsightsQueryHandler
    : IQueryHandler<RunLogInsightsQuery, RunLogInsightsQueryResult>
{
    private readonly ICloudWatchLogsClient _client;
    private readonly ILogger _logger;

    public RunLogInsightsQueryHandler(ICloudWatchLogsClient client, ILogger<RunLogInsightsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<RunLogInsightsQueryResult>> Handle(
        RunLogInsightsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.LogGroupName);
        var result = await _client.RunInsightsQueryAsync(
            request.LogGroupName,
            request.QueryString,
            request.StartTime,
            request.EndTime,
            request.Limit,
            cancellationToken);
        LogHandled(result.IsSuccess);

        if (!result.IsSuccess)
        {
            Result<RunLogInsightsQueryResult> failure = result.Error!.Value;
            return failure;
        }

        return new RunLogInsightsQueryResult(result.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Running CloudWatch Logs Insights query for {LogGroupName}.")]
    private partial void LogHandling(string logGroupName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch Logs Insights query handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
