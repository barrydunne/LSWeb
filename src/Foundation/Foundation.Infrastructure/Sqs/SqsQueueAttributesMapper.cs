using System.Globalization;
using Foundation.Domain.Sqs;

namespace Foundation.Infrastructure.Sqs;

/// <summary>
/// Translates the SQS attribute bag returned by the backend into the strongly-typed
/// <see cref="SqsQueueAttributes"/> domain record, applying safe defaults for any attribute the
/// backend leaves unset.
/// </summary>
internal static class SqsQueueAttributesMapper
{
    private const string VisibilityTimeout = "VisibilityTimeout";
    private const string MessageRetentionPeriod = "MessageRetentionPeriod";
    private const string DelaySeconds = "DelaySeconds";
    private const string ReceiveMessageWaitTimeSeconds = "ReceiveMessageWaitTimeSeconds";
    private const string MaximumMessageSize = "MaximumMessageSize";
    private const string QueueArn = "QueueArn";
    private const string FifoQueue = "FifoQueue";
    private const string ApproximateNumberOfMessages = "ApproximateNumberOfMessages";
    private const string ApproximateNumberOfMessagesNotVisible = "ApproximateNumberOfMessagesNotVisible";
    private const string ApproximateNumberOfMessagesDelayed = "ApproximateNumberOfMessagesDelayed";

    /// <summary>
    /// Map an SQS attribute bag to the domain attributes record.
    /// </summary>
    /// <param name="attributes">The queue attributes reported by the backend; may be null or sparse.</param>
    /// <returns>The domain attributes.</returns>
    public static SqsQueueAttributes ToAttributes(IReadOnlyDictionary<string, string>? attributes)
        => new(
            ReadInt(attributes, VisibilityTimeout),
            ReadInt(attributes, MessageRetentionPeriod),
            ReadInt(attributes, DelaySeconds),
            ReadInt(attributes, ReceiveMessageWaitTimeSeconds),
            ReadInt(attributes, MaximumMessageSize),
            ReadString(attributes, QueueArn),
            ReadBool(attributes, FifoQueue),
            ReadLong(attributes, ApproximateNumberOfMessages),
            ReadLong(attributes, ApproximateNumberOfMessagesNotVisible),
            ReadLong(attributes, ApproximateNumberOfMessagesDelayed));

    private static int ReadInt(IReadOnlyDictionary<string, string>? attributes, string key)
        => attributes is not null
            && attributes.TryGetValue(key, out var value)
            && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0;

    private static long ReadLong(IReadOnlyDictionary<string, string>? attributes, string key)
        => attributes is not null
            && attributes.TryGetValue(key, out var value)
            && long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0;

    private static string ReadString(IReadOnlyDictionary<string, string>? attributes, string key)
        => attributes is not null && attributes.TryGetValue(key, out var value) ? value : string.Empty;

    private static bool ReadBool(IReadOnlyDictionary<string, string>? attributes, string key)
        => attributes is not null
            && attributes.TryGetValue(key, out var value)
            && bool.TryParse(value, out var parsed)
            && parsed;
}
