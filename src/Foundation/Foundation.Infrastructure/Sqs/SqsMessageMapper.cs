using Amazon.SQS.Model;
using SqsMessage = Foundation.Domain.Sqs.SqsMessage;

namespace Foundation.Infrastructure.Sqs;

/// <summary>
/// Translates AWS SQS message shapes into the domain records the application works with, projecting
/// message attributes to their string values and applying safe defaults for missing collections.
/// </summary>
internal static class SqsMessageMapper
{
    /// <summary>
    /// Map an SDK message to the domain message.
    /// </summary>
    /// <param name="message">The SDK message returned by a receive call.</param>
    /// <returns>The domain message.</returns>
    public static SqsMessage ToMessage(Message message)
        => new(
            message.MessageId ?? string.Empty,
            message.ReceiptHandle ?? string.Empty,
            message.Body ?? string.Empty,
            message.Attributes is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(message.Attributes),
            ProjectMessageAttributes(message.MessageAttributes));

    private static Dictionary<string, string> ProjectMessageAttributes(
        Dictionary<string, MessageAttributeValue>? attributes)
    {
        var projected = new Dictionary<string, string>();
        if (attributes is null)
            return projected;

        foreach (var (key, value) in attributes)
            projected[key] = value.StringValue ?? value.DataType ?? string.Empty;

        return projected;
    }
}
