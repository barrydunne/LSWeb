using System.Text.Json;
using Foundation.Domain.Navigation;
using Foundation.Domain.Sqs;

namespace Foundation.Infrastructure.Sqs;

/// <summary>
/// Extracts dead-letter queue relationships from a queue's <c>RedrivePolicy</c> and
/// <c>RedriveAllowPolicy</c> attributes. The redrive policy names the dead-letter queue a queue
/// feeds (a source-to-DLQ link), while the redrive-allow policy, when scoped <c>byQueue</c>, names
/// the source queues permitted to use the queue as their dead-letter queue (DLQ-to-source links).
/// Both are projected to records so the relationships can be shown as cross-resource links.
/// </summary>
internal static class SqsRedriveMapper
{
    private const string DeadLetterTargetArnKey = "deadLetterTargetArn";
    private const string MaxReceiveCountKey = "maxReceiveCount";
    private const string SourceQueueArnsKey = "sourceQueueArns";

    /// <summary>
    /// Parse the dead-letter queue relationships of a queue from its redrive policies.
    /// </summary>
    /// <param name="redrivePolicyJson">The queue's <c>RedrivePolicy</c> attribute, or <see langword="null"/> when none is set.</param>
    /// <param name="redriveAllowPolicyJson">The queue's <c>RedriveAllowPolicy</c> attribute, or <see langword="null"/> when none is set.</param>
    /// <returns>The redrive relationships; the target is <see langword="null"/> and the sources empty when none are found or the policies are unreadable.</returns>
    public static SqsRedrive ParseRedrive(string? redrivePolicyJson, string? redriveAllowPolicyJson)
        => new(ParseTarget(redrivePolicyJson), ParseSources(redriveAllowPolicyJson));

    private static SqsRedriveTarget? ParseTarget(string? redrivePolicyJson)
    {
        if (string.IsNullOrWhiteSpace(redrivePolicyJson))
            return null;

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(redrivePolicyJson);
        }
        catch (JsonException)
        {
            return null;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty(DeadLetterTargetArnKey, out var arnElement)
                || arnElement.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var arn = arnElement.GetString()!;
            var name = ArnParts.TryParse(arn, out var parts) ? parts.ResourceId : arn;
            var maxReceiveCount = ReadMaxReceiveCount(document.RootElement);

            return new SqsRedriveTarget(arn, name, maxReceiveCount);
        }
    }

    private static int ReadMaxReceiveCount(JsonElement root)
    {
        if (!root.TryGetProperty(MaxReceiveCountKey, out var element))
            return 0;

        return element.ValueKind switch
        {
            JsonValueKind.Number when element.TryGetInt32(out var number) => number,
            JsonValueKind.String when int.TryParse(element.GetString(), out var parsed) => parsed,
            _ => 0,
        };
    }

    private static List<SqsRedriveSource> ParseSources(string? redriveAllowPolicyJson)
    {
        var sources = new List<SqsRedriveSource>();
        if (string.IsNullOrWhiteSpace(redriveAllowPolicyJson))
            return sources;

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(redriveAllowPolicyJson);
        }
        catch (JsonException)
        {
            return sources;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty(SourceQueueArnsKey, out var arns)
                || arns.ValueKind != JsonValueKind.Array)
            {
                return sources;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var element in arns.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.String)
                    continue;

                var arn = element.GetString()!;
                if (!seen.Add(arn))
                    continue;

                var name = ArnParts.TryParse(arn, out var parts) ? parts.ResourceId : arn;
                sources.Add(new SqsRedriveSource(arn, name));
            }
        }

        return sources;
    }
}
