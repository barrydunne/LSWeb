using System.Text.Json;
using Foundation.Domain.Lambda;
using Foundation.Domain.Navigation;

namespace Foundation.Infrastructure.Lambda;

/// <summary>
/// Extracts S3-to-Lambda trigger relationships from a function's resource-based policy. When an S3
/// bucket is configured to invoke a function, a policy statement grants the S3 service permission to
/// invoke it, guarded by an <c>AWS:SourceArn</c> condition naming the bucket. This reads those
/// source ARNs and projects the S3 ones into <see cref="LambdaS3Trigger"/> records so the trigger
/// can be shown alongside the function's event source mappings.
/// </summary>
internal static class LambdaPolicyMapper
{
    private const string SourceArnConditionKey = "AWS:SourceArn";
    private const string S3ServiceNamespace = "s3";

    /// <summary>
    /// Parse the S3 buckets that trigger a function from its resource-based policy document.
    /// </summary>
    /// <param name="policyJson">The function's policy document, or <see langword="null"/> when none is set.</param>
    /// <returns>The distinct S3 triggers, in first-seen order; empty when none are found or the policy is unreadable.</returns>
    public static IReadOnlyList<LambdaS3Trigger> ParseS3Triggers(string? policyJson)
    {
        var triggers = new List<LambdaS3Trigger>();
        if (string.IsNullOrWhiteSpace(policyJson))
            return triggers;

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(policyJson);
        }
        catch (JsonException)
        {
            return triggers;
        }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("Statement", out var statement))
                return triggers;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var element in EnumerateElements(statement))
                CollectSourceArns(element, triggers, seen);
        }

        return triggers;
    }

    private static void CollectSourceArns(
        JsonElement statement, List<LambdaS3Trigger> triggers, HashSet<string> seen)
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
                    AddS3Trigger(arn, triggers, seen);
            }
        }
    }

    private static void AddS3Trigger(
        string arn, List<LambdaS3Trigger> triggers, HashSet<string> seen)
    {
        if (!ArnParts.TryParse(arn, out var parts)
            || !string.Equals(parts.Service, S3ServiceNamespace, StringComparison.OrdinalIgnoreCase)
            || !seen.Add(arn))
        {
            return;
        }

        triggers.Add(new LambdaS3Trigger(arn));
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
