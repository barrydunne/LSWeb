using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.GetLambdaInvocationInsights;

/// <summary>
/// Derive invocation monitoring information for a Lambda function from its recent CloudWatch log events.
/// </summary>
/// <param name="FunctionName">The name of the function to analyse.</param>
/// <param name="Limit">The maximum number of log events to analyse; non-positive values fall back to a default.</param>
public record GetLambdaInvocationInsightsQuery(string FunctionName, int Limit) : IQuery<GetLambdaInvocationInsightsQueryResult>;

/// <summary>
/// The derived invocation insights for a Lambda function.
/// </summary>
/// <param name="LogGroupName">The CloudWatch log group the events were read from.</param>
/// <param name="Insights">The derived metrics and recent invocations.</param>
public record GetLambdaInvocationInsightsQueryResult(
    string LogGroupName,
    LambdaInvocationInsights Insights);
