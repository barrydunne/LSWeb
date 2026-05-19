using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.ListLambdaLogEvents;

/// <summary>
/// Read the most recent CloudWatch log events for a Lambda function's log group.
/// </summary>
/// <param name="FunctionName">The name of the function whose log events to read.</param>
/// <param name="Limit">The maximum number of log events to return; non-positive values fall back to a default.</param>
public record ListLambdaLogEventsQuery(string FunctionName, int Limit) : IQuery<ListLambdaLogEventsQueryResult>;

/// <summary>
/// The most recent CloudWatch log events for a Lambda function.
/// </summary>
/// <param name="LogGroupName">The CloudWatch log group the events were read from.</param>
/// <param name="Events">The log events, ordered oldest first.</param>
public record ListLambdaLogEventsQueryResult(
    string LogGroupName,
    IReadOnlyList<LambdaLogEvent> Events);
