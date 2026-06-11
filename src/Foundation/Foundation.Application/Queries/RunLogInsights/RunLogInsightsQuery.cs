using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudWatchLogs;

namespace Foundation.Application.Queries.RunLogInsights;

/// <summary>
/// Run a CloudWatch Logs Insights query against a log group over a time range and wait for the
/// matching rows. Powers the embedded Insights query builder.
/// </summary>
/// <param name="LogGroupName">The name of the log group to query.</param>
/// <param name="QueryString">The CloudWatch Logs Insights query to run.</param>
/// <param name="StartTime">The inclusive lower bound of the query time range.</param>
/// <param name="EndTime">The inclusive upper bound of the query time range.</param>
/// <param name="Limit">The maximum number of result rows to return.</param>
public record RunLogInsightsQuery(
    string LogGroupName,
    string QueryString,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int Limit)
    : IQuery<RunLogInsightsQueryResult>;

/// <summary>
/// The result of a CloudWatch Logs Insights query.
/// </summary>
/// <param name="Result">The Insights query outcome and matching rows.</param>
public record RunLogInsightsQueryResult(LogInsightsResult Result);
