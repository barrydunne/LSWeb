using System.Diagnostics.CodeAnalysis;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Foundation.Domain.EventBridge;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.EventBridge;

/// <summary>
/// Reads EventBridge through the resilient AWS gateway so the same code works against LocalStack or
/// real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and
/// converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class EventBridgeClientAdapter : IEventBridgeClient
{
    private const string ServiceKey = "eventbridge";

    private readonly IAwsGateway _gateway;

    public EventBridgeClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<EventBridgeRule>>> ListRulesAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonEventBridgeClient, IReadOnlyList<EventBridgeRule>>(
            ServiceKey,
            async (client, token) =>
            {
                var rules = new List<EventBridgeRule>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListRulesAsync(
                        new ListRulesRequest { NextToken = nextToken },
                        token);

                    foreach (var rule in response.Rules ?? [])
                        rules.Add(ToRule(rule));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return rules;
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<EventBridgeTarget>>> ListTargetsByRuleAsync(
        string ruleName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonEventBridgeClient, IReadOnlyList<EventBridgeTarget>>(
            ServiceKey,
            async (client, token) =>
            {
                var targets = new List<EventBridgeTarget>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListTargetsByRuleAsync(
                        new ListTargetsByRuleRequest { Rule = ruleName, NextToken = nextToken },
                        token);

                    foreach (var target in response.Targets ?? [])
                        targets.Add(new EventBridgeTarget(
                            target.Id ?? string.Empty,
                            target.Arn ?? string.Empty));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return targets;
            },
            cancellationToken);

    public Task<Result<EventBridgeRuleDetail>> DescribeRuleAsync(
        string ruleName, string? eventBusName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonEventBridgeClient, EventBridgeRuleDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new DescribeRuleRequest { Name = ruleName };
                if (!string.IsNullOrWhiteSpace(eventBusName))
                    request.EventBusName = eventBusName;

                var response = await client.DescribeRuleAsync(request, token);

                return new EventBridgeRuleDetail(
                    response.Name ?? string.Empty,
                    response.Arn ?? string.Empty,
                    response.EventBusName ?? string.Empty,
                    response.State?.Value ?? string.Empty,
                    string.IsNullOrWhiteSpace(response.ScheduleExpression) ? null : response.ScheduleExpression,
                    string.IsNullOrWhiteSpace(response.Description) ? null : response.Description,
                    string.IsNullOrWhiteSpace(response.RoleArn) ? null : response.RoleArn,
                    string.IsNullOrWhiteSpace(response.ManagedBy) ? null : response.ManagedBy,
                    string.IsNullOrWhiteSpace(response.EventPattern) ? null : response.EventPattern);
            },
            cancellationToken);

    public Task<Result<EventBridgePutResult>> PutEventAsync(
        string source,
        string detailType,
        string detail,
        string? eventBusName,
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonEventBridgeClient, EventBridgePutResult>(
            ServiceKey,
            async (client, token) =>
            {
                var entry = new PutEventsRequestEntry
                {
                    Source = source,
                    DetailType = detailType,
                    Detail = detail,
                };
                if (!string.IsNullOrWhiteSpace(eventBusName))
                    entry.EventBusName = eventBusName;

                var response = await client.PutEventsAsync(
                    new PutEventsRequest { Entries = [entry] },
                    token);

                var resultEntry = response.Entries?.FirstOrDefault();
                return new EventBridgePutResult(
                    string.IsNullOrWhiteSpace(resultEntry?.EventId) ? null : resultEntry.EventId,
                    response.FailedEntryCount ?? 0,
                    string.IsNullOrWhiteSpace(resultEntry?.ErrorCode) ? null : resultEntry.ErrorCode,
                    string.IsNullOrWhiteSpace(resultEntry?.ErrorMessage) ? null : resultEntry.ErrorMessage);
            },
            cancellationToken);

    public async Task<Result> PutRuleAsync(
        EventBridgeRuleSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonEventBridgeClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new PutRuleRequest
                {
                    Name = specification.Name,
                    ScheduleExpression = specification.ScheduleExpression,
                    State = RuleState.FindValue(specification.State),
                };
                if (!string.IsNullOrWhiteSpace(specification.Description))
                    request.Description = specification.Description;
                if (!string.IsNullOrWhiteSpace(specification.EventBusName))
                    request.EventBusName = specification.EventBusName;

                await client.PutRuleAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> PutEventPatternRuleAsync(
        EventBridgeRulePatternSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonEventBridgeClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new PutRuleRequest
                {
                    Name = specification.Name,
                    EventPattern = specification.EventPattern,
                    State = RuleState.FindValue(specification.State),
                };
                if (!string.IsNullOrWhiteSpace(specification.Description))
                    request.Description = specification.Description;
                if (!string.IsNullOrWhiteSpace(specification.EventBusName))
                    request.EventBusName = specification.EventBusName;

                await client.PutRuleAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteRuleAsync(
        string ruleName, string? eventBusName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonEventBridgeClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new DeleteRuleRequest { Name = ruleName };
                if (!string.IsNullOrWhiteSpace(eventBusName))
                    request.EventBusName = eventBusName;

                await client.DeleteRuleAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> EnableRuleAsync(
        string ruleName, string? eventBusName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonEventBridgeClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new EnableRuleRequest { Name = ruleName };
                if (!string.IsNullOrWhiteSpace(eventBusName))
                    request.EventBusName = eventBusName;

                await client.EnableRuleAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DisableRuleAsync(
        string ruleName, string? eventBusName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonEventBridgeClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new DisableRuleRequest { Name = ruleName };
                if (!string.IsNullOrWhiteSpace(eventBusName))
                    request.EventBusName = eventBusName;

                await client.DisableRuleAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> PutTargetsAsync(
        string ruleName,
        string? eventBusName,
        IReadOnlyList<EventBridgeTargetSpecification> targets,
        CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonEventBridgeClient, Result>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new PutTargetsRequest
                {
                    Rule = ruleName,
                    Targets = targets
                        .Select(ToTarget)
                        .ToList(),
                };
                if (!string.IsNullOrWhiteSpace(eventBusName))
                    request.EventBusName = eventBusName;

                var response = await client.PutTargetsAsync(request, token);
                return ToOutcome(response.FailedEntryCount ?? 0, response.FailedEntries?.FirstOrDefault()?.ErrorMessage);
            },
            cancellationToken);

        return result.IsSuccess ? result.Value : result.Error!.Value;
    }

    public async Task<Result> RemoveTargetsAsync(
        string ruleName,
        string? eventBusName,
        IReadOnlyList<string> targetIds,
        CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonEventBridgeClient, Result>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new RemoveTargetsRequest
                {
                    Rule = ruleName,
                    Ids = targetIds.ToList(),
                };
                if (!string.IsNullOrWhiteSpace(eventBusName))
                    request.EventBusName = eventBusName;

                var response = await client.RemoveTargetsAsync(request, token);
                return ToOutcome(response.FailedEntryCount ?? 0, response.FailedEntries?.FirstOrDefault()?.ErrorMessage);
            },
            cancellationToken);

        return result.IsSuccess ? result.Value : result.Error!.Value;
    }

    private static Result ToOutcome(int failedEntryCount, string? errorMessage)
        => failedEntryCount > 0
            ? new Error($"EventBridge reported {failedEntryCount} failed target entr{(failedEntryCount == 1 ? "y" : "ies")}: {errorMessage ?? "unknown error"}.")
            : Result.Success();

    public Task<Result<IReadOnlyList<EventBridgeEventBus>>> ListEventBusesAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonEventBridgeClient, IReadOnlyList<EventBridgeEventBus>>(
            ServiceKey,
            async (client, token) =>
            {
                var buses = new List<EventBridgeEventBus>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListEventBusesAsync(
                        new ListEventBusesRequest { NextToken = nextToken }, token);

                    foreach (var bus in response.EventBuses ?? [])
                        buses.Add(new EventBridgeEventBus(
                            bus.Name ?? string.Empty,
                            bus.Arn ?? string.Empty));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return buses;
            },
            cancellationToken);

    public async Task<Result> CreateEventBusAsync(string name, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonEventBridgeClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateEventBusAsync(new CreateEventBusRequest { Name = name }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteEventBusAsync(string name, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonEventBridgeClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteEventBusAsync(new DeleteEventBusRequest { Name = name }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static Target ToTarget(EventBridgeTargetSpecification specification)
    {
        var target = new Target
        {
            Id = specification.Id,
            Arn = specification.Arn,
        };
        if (!string.IsNullOrWhiteSpace(specification.RoleArn))
            target.RoleArn = specification.RoleArn;
        if (!string.IsNullOrWhiteSpace(specification.Input))
            target.Input = specification.Input;

        return target;
    }

    private static EventBridgeRule ToRule(Rule rule)
        => new(
            rule.Name ?? string.Empty,
            rule.Arn ?? string.Empty,
            rule.EventBusName ?? string.Empty,
            rule.State?.Value ?? string.Empty,
            string.IsNullOrWhiteSpace(rule.Description) ? null : rule.Description,
            string.IsNullOrWhiteSpace(rule.ScheduleExpression) ? null : rule.ScheduleExpression);
}
