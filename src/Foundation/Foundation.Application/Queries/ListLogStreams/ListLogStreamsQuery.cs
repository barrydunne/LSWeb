using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudWatchLogs;

namespace Foundation.Application.Queries.ListLogStreams;

/// <summary>
/// List the log streams within a CloudWatch log group.
/// </summary>
/// <param name="LogGroupName">The name of the log group to inspect.</param>
public record ListLogStreamsQuery(string LogGroupName) : IQuery<ListLogStreamsQueryResult>;

/// <summary>
/// The CloudWatch log streams returned by the backend.
/// </summary>
/// <param name="LogStreams">The log streams, most recently active first.</param>
public record ListLogStreamsQueryResult(IReadOnlyList<LogStream> LogStreams);
