namespace Foundation.Domain.Sqs;

/// <summary>
/// A message retrieved from an SQS queue, including its body and the receipt handle required to
/// delete it. Both the system attributes and any custom message attributes are exposed as strings.
/// </summary>
/// <param name="MessageId">The backend-assigned identifier for the message.</param>
/// <param name="ReceiptHandle">The handle returned by the receive call, used to delete the message.</param>
/// <param name="Body">The message body as returned by the backend.</param>
/// <param name="Attributes">The system attributes reported for the message (for example SentTimestamp).</param>
/// <param name="MessageAttributes">The custom message attributes, projected to their string values.</param>
public sealed record SqsMessage(
    string MessageId,
    string ReceiptHandle,
    string Body,
    IReadOnlyDictionary<string, string> Attributes,
    IReadOnlyDictionary<string, string> MessageAttributes);
