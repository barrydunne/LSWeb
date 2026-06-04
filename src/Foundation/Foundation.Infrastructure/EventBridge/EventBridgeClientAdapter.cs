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

    private static EventBridgeRule ToRule(Rule rule)
        => new(
            rule.Name ?? string.Empty,
            rule.Arn ?? string.Empty,
            rule.EventBusName ?? string.Empty,
            rule.State?.Value ?? string.Empty,
            string.IsNullOrWhiteSpace(rule.Description) ? null : rule.Description,
            string.IsNullOrWhiteSpace(rule.ScheduleExpression) ? null : rule.ScheduleExpression);
}
