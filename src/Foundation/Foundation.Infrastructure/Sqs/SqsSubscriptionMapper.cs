using System.Text.Json;
using Foundation.Domain.Navigation;
using Foundation.Domain.Sqs;

namespace Foundation.Infrastructure.Sqs;

/// <summary>
/// Extracts SNS-to-SQS subscription relationships from a queue's access policy. When a queue is
/// subscribed to an SNS topic, a policy statement grants the topic permission to send messages,
/// guarded by an <c>aws:SourceArn</c> condition naming the topic. This reads those source ARNs and
/// projects the SNS ones into <see cref="SqsQueueSubscription"/> records so the relationship can be
/// shown as a cross-resource link.
/// </summary>
internal static class SqsSubscriptionMapper
{
    private const string SourceArnConditionKey = "aws:SourceArn";
    private const string SnsServiceNamespace = "sns";

    /// <summary>
    /// Parse the SNS topics that publish to a queue from its access policy document.
    /// </summary>
    /// <param name="policyJson">The queue's <c>Policy</c> attribute, or <see langword="null"/> when none is set.</param>
    /// <returns>The distinct SNS subscriptions, in first-seen order; empty when none are found or the policy is unreadable.</returns>
    public static IReadOnlyList<SqsQueueSubscription> ParseSubscriptions(string? policyJson)
    {
        var subscriptions = new List<SqsQueueSubscription>();
        if (string.IsNullOrWhiteSpace(policyJson))
            return subscriptions;

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(policyJson);
        }
        catch (JsonException)
        {
            return subscriptions;
        }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("Statement", out var statement))
                return subscriptions;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var element in EnumerateElements(statement))
                CollectSourceArns(element, subscriptions, seen);
        }

        return subscriptions;
    }

    private static void CollectSourceArns(
        JsonElement statement, List<SqsQueueSubscription> subscriptions, HashSet<string> seen)
    {
        if (statement.ValueKind != JsonValueKind.Object
            || !statement.TryGetProperty("Condition", out var condition)
            || condition.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var operatorEntry in condition.EnumerateObject())
        {
            if (operatorEntry.Value.ValueKind != JsonValueKind.Object)
                continue;

            foreach (var keyEntry in operatorEntry.Value.EnumerateObject())
            {
                if (!string.Equals(keyEntry.Name, SourceArnConditionKey, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var arn in EnumerateStrings(keyEntry.Value))
                    AddSnsTopic(arn, subscriptions, seen);
            }
        }
    }

    private static void AddSnsTopic(
        string arn, List<SqsQueueSubscription> subscriptions, HashSet<string> seen)
    {
        if (!ArnParts.TryParse(arn, out var parts)
            || !string.Equals(parts.Service, SnsServiceNamespace, StringComparison.OrdinalIgnoreCase)
            || !seen.Add(arn))
        {
            return;
        }

        subscriptions.Add(new SqsQueueSubscription(arn, parts.ResourceId));
    }

    private static IEnumerable<JsonElement> EnumerateElements(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                yield return item;
        }
        else
        {
            yield return element;
        }
    }

    private static IEnumerable<string> EnumerateStrings(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                        yield return item.GetString()!;
                }

                break;
            case JsonValueKind.String:
                yield return element.GetString()!;
                break;
        }
    }
}
