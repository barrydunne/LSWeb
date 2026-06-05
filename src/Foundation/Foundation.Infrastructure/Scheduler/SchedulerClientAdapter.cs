using System.Diagnostics.CodeAnalysis;
using Amazon.Scheduler;
using Amazon.Scheduler.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Scheduler;
using Foundation.Domain.Scheduler;
using Foundation.Infrastructure.Aws;
using DomainScheduleDetail = Foundation.Domain.Scheduler.ScheduleDetail;
using DomainScheduleSummary = Foundation.Domain.Scheduler.ScheduleSummary;

namespace Foundation.Infrastructure.Scheduler;

/// <summary>
/// Reads EventBridge Scheduler schedules through the resilient AWS gateway so the same code works
/// against LocalStack or real AWS. All access flows through <see cref="IAwsGateway"/>, which records
/// capability and converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class SchedulerClientAdapter : ISchedulerClient
{
    private const string ServiceKey = "scheduler";

    private readonly IAwsGateway _gateway;

    public SchedulerClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<DomainScheduleSummary>>> ListSchedulesAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSchedulerClient, IReadOnlyList<DomainScheduleSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var schedules = new List<DomainScheduleSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListSchedulesAsync(
                        new ListSchedulesRequest { NextToken = nextToken },
                        token);

                    foreach (var schedule in response.Schedules ?? [])
                        schedules.Add(ToSummary(schedule));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return schedules;
            },
            cancellationToken);

    public Task<Result<DomainScheduleDetail>> GetScheduleAsync(
        string name, string groupName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSchedulerClient, DomainScheduleDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetScheduleAsync(
                    new GetScheduleRequest { Name = name, GroupName = groupName },
                    token);

                return new DomainScheduleDetail(
                    response.Name ?? string.Empty,
                    response.GroupName ?? string.Empty,
                    response.State?.Value ?? string.Empty,
                    response.ScheduleExpression ?? string.Empty,
                    string.IsNullOrEmpty(response.ScheduleExpressionTimezone) ? null : response.ScheduleExpressionTimezone,
                    string.IsNullOrEmpty(response.Description) ? null : response.Description,
                    ToTimestamp(response.StartDate),
                    ToTimestamp(response.EndDate),
                    response.Target?.Arn ?? string.Empty,
                    response.Target?.RoleArn ?? string.Empty,
                    response.FlexibleTimeWindow?.Mode?.Value ?? string.Empty,
                    response.FlexibleTimeWindow?.MaximumWindowInMinutes,
                    response.Arn ?? string.Empty,
                    ToTimestamp(response.CreationDate),
                    ToTimestamp(response.LastModificationDate));
            },
            cancellationToken);

    public async Task<Result> CreateScheduleAsync(
        ScheduleSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSchedulerClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateScheduleRequest
                {
                    Name = specification.Name,
                    GroupName = specification.GroupName,
                    ScheduleExpression = specification.ScheduleExpression,
                    State = ScheduleState.FindValue(specification.State),
                    Target = new Target
                    {
                        Arn = specification.TargetArn,
                        RoleArn = specification.RoleArn,
                    },
                    FlexibleTimeWindow = ToFlexibleTimeWindow(specification),
                };

                ApplyOptional(request, specification);

                await client.CreateScheduleAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> UpdateScheduleAsync(
        ScheduleSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSchedulerClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new UpdateScheduleRequest
                {
                    Name = specification.Name,
                    GroupName = specification.GroupName,
                    ScheduleExpression = specification.ScheduleExpression,
                    State = ScheduleState.FindValue(specification.State),
                    Target = new Target
                    {
                        Arn = specification.TargetArn,
                        RoleArn = specification.RoleArn,
                    },
                    FlexibleTimeWindow = ToFlexibleTimeWindow(specification),
                };

                ApplyOptional(request, specification);

                await client.UpdateScheduleAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteScheduleAsync(
        string name, string groupName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSchedulerClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteScheduleAsync(
                    new DeleteScheduleRequest { Name = name, GroupName = groupName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<ScheduleGroup>>> ListScheduleGroupsAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSchedulerClient, IReadOnlyList<ScheduleGroup>>(
            ServiceKey,
            async (client, token) =>
            {
                var groups = new List<ScheduleGroup>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListScheduleGroupsAsync(
                        new ListScheduleGroupsRequest { NextToken = nextToken },
                        token);

                    foreach (var group in response.ScheduleGroups ?? [])
                        groups.Add(ToGroup(group));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return groups;
            },
            cancellationToken);

    public async Task<Result> CreateScheduleGroupAsync(string name, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSchedulerClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateScheduleGroupAsync(
                    new CreateScheduleGroupRequest { Name = name },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteScheduleGroupAsync(string name, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSchedulerClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteScheduleGroupAsync(
                    new DeleteScheduleGroupRequest { Name = name },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static ScheduleGroup ToGroup(ScheduleGroupSummary group)
        => new(
            group.Name ?? string.Empty,
            group.State?.Value ?? string.Empty,
            group.Arn ?? string.Empty,
            ToTimestamp(group.CreationDate),
            ToTimestamp(group.LastModificationDate));

    private static FlexibleTimeWindow ToFlexibleTimeWindow(ScheduleSpecification specification)
        => new()
        {
            Mode = FlexibleTimeWindowMode.FindValue(specification.FlexibleTimeWindowMode),
            MaximumWindowInMinutes = specification.MaximumWindowInMinutes,
        };

    private static void ApplyOptional(CreateScheduleRequest request, ScheduleSpecification specification)
    {
        if (!string.IsNullOrEmpty(specification.ScheduleExpressionTimezone))
            request.ScheduleExpressionTimezone = specification.ScheduleExpressionTimezone;
        if (!string.IsNullOrEmpty(specification.Description))
            request.Description = specification.Description;
        if (specification.StartDate is not null)
            request.StartDate = specification.StartDate.Value.UtcDateTime;
        if (specification.EndDate is not null)
            request.EndDate = specification.EndDate.Value.UtcDateTime;
    }

    private static void ApplyOptional(UpdateScheduleRequest request, ScheduleSpecification specification)
    {
        if (!string.IsNullOrEmpty(specification.ScheduleExpressionTimezone))
            request.ScheduleExpressionTimezone = specification.ScheduleExpressionTimezone;
        if (!string.IsNullOrEmpty(specification.Description))
            request.Description = specification.Description;
        if (specification.StartDate is not null)
            request.StartDate = specification.StartDate.Value.UtcDateTime;
        if (specification.EndDate is not null)
            request.EndDate = specification.EndDate.Value.UtcDateTime;
    }

    private static DomainScheduleSummary ToSummary(Amazon.Scheduler.Model.ScheduleSummary schedule)
        => new(
            schedule.Name ?? string.Empty,
            schedule.GroupName ?? string.Empty,
            schedule.State?.Value ?? string.Empty,
            schedule.Target?.Arn ?? string.Empty,
            schedule.Arn ?? string.Empty);

    private static DateTimeOffset? ToTimestamp(DateTime? value)
        => value is null
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
}
