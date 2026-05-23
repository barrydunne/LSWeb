using System.Globalization;
using Foundation.Domain.Sqs;

namespace Foundation.Infrastructure.Sqs;

/// <summary>
/// Translates AWS SQS SDK shapes into the domain records the application works with, applying safe
/// defaults for any attribute the backend leaves unset.
/// </summary>
internal static class SqsQueueMapper
{
    private const string ApproximateNumberOfMessages = "ApproximateNumberOfMessages";
    private const string ApproximateNumberOfMessagesNotVisible = "ApproximateNumberOfMessagesNotVisible";
    private const string ApproximateNumberOfMessagesDelayed = "ApproximateNumberOfMessagesDelayed";

    /// <summary>
    /// Map a queue URL and its attribute bag to the domain queue, deriving the queue name from the URL.
    /// </summary>
    /// <param name="queueUrl">The fully-qualified queue URL.</param>
    /// <param name="attributes">The queue attributes reported by the backend; may be null or sparse.</param>
    /// <returns>The domain queue.</returns>
    public static SqsQueue ToQueue(string queueUrl, IReadOnlyDictionary<string, string>? attributes)
        => new(
            DeriveName(queueUrl),
            queueUrl,
            ReadLong(attributes, ApproximateNumberOfMessages),
            ReadLong(attributes, ApproximateNumberOfMessagesNotVisible),
            ReadLong(attributes, ApproximateNumberOfMessagesDelayed));

    private static string DeriveName(string queueUrl)
    {
        var lastSlash = queueUrl.LastIndexOf('/');
        return lastSlash >= 0 && lastSlash < queueUrl.Length - 1
            ? queueUrl[(lastSlash + 1)..]
            : queueUrl;
    }

    private static long ReadLong(IReadOnlyDictionary<string, string>? attributes, string key)
        => attributes is not null
            && attributes.TryGetValue(key, out var value)
            && long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0;
}
