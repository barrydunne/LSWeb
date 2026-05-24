using System.Diagnostics.CodeAnalysis;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Foundation.Infrastructure.Aws;
using LogEvent = Foundation.Domain.CloudWatchLogs.LogEvent;
using LogGroup = Foundation.Domain.CloudWatchLogs.LogGroup;
using LogStream = Foundation.Domain.CloudWatchLogs.LogStream;

namespace Foundation.Infrastructure.CloudWatchLogs;

/// <summary>
/// Reads CloudWatch Logs through the resilient AWS gateway so the same code works against LocalStack
/// or real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and
/// converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class CloudWatchLogsClientAdapter : ICloudWatchLogsClient
{
    private const string ServiceKey = "cloudwatch-logs";

    private readonly IAwsGateway _gateway;

    public CloudWatchLogsClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<LogGroup>>> ListLogGroupsAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudWatchLogsClient, IReadOnlyList<LogGroup>>(
            ServiceKey,
            async (client, token) =>
            {
                var groups = new List<LogGroup>();
                string? nextToken = null;

                do
                {
                    var response = await client.DescribeLogGroupsAsync(
                        new DescribeLogGroupsRequest { NextToken = nextToken, Limit = 50 },
                        token);

                    foreach (var group in response.LogGroups ?? [])
                        groups.Add(LogGroupMapper.ToLogGroup(group));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return groups;
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<LogStream>>> ListLogStreamsAsync(
        string logGroupName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudWatchLogsClient, IReadOnlyList<LogStream>>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.DescribeLogStreamsAsync(
                    new DescribeLogStreamsRequest
                    {
                        LogGroupName = logGroupName,
                        OrderBy = OrderBy.LastEventTime,
                        Descending = true,
                        Limit = 50,
                    },
                    token);

                IReadOnlyList<LogStream> streams = (response.LogStreams ?? [])
                    .Select(LogStreamMapper.ToLogStream)
                    .ToList();
                return streams;
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<LogEvent>>> GetLogEventsAsync(
        string logGroupName, string logStreamName, int limit, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudWatchLogsClient, IReadOnlyList<LogEvent>>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetLogEventsAsync(
                    new GetLogEventsRequest
                    {
                        LogGroupName = logGroupName,
                        LogStreamName = logStreamName,
                        Limit = Math.Clamp(limit, 1, 1000),
                        StartFromHead = false,
                    },
                    token);

                IReadOnlyList<LogEvent> events = (response.Events ?? [])
                    .Select(LogEventMapper.ToLogEvent)
                    .ToList();
                return events;
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<LogEvent>>> FilterLogEventsAsync(
        string logGroupName, string? filterPattern, DateTimeOffset? startTime, int limit, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudWatchLogsClient, IReadOnlyList<LogEvent>>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.FilterLogEventsAsync(
                    new FilterLogEventsRequest
                    {
                        LogGroupName = logGroupName,
                        FilterPattern = string.IsNullOrWhiteSpace(filterPattern) ? null : filterPattern,
                        StartTime = startTime?.ToUnixTimeMilliseconds(),
                        Limit = Math.Clamp(limit, 1, 1000),
                    },
                    token);

                IReadOnlyList<LogEvent> events = (response.Events ?? [])
                    .Select(LogEventMapper.ToFilteredLogEvent)
                    .ToList();
                return events;
            },
            cancellationToken);

    public async Task<Result> CreateLogGroupAsync(string logGroupName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonCloudWatchLogsClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateLogGroupAsync(
                    new CreateLogGroupRequest { LogGroupName = logGroupName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteLogGroupAsync(string logGroupName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonCloudWatchLogsClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteLogGroupAsync(
                    new DeleteLogGroupRequest { LogGroupName = logGroupName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }
}
