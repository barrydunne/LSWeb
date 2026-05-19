namespace Foundation.Domain.Lambda;

/// <summary>
/// A single log event emitted by a Lambda function into its CloudWatch log group.
/// </summary>
/// <param name="Timestamp">The time the event was recorded, in ISO 8601 form; empty when not reported.</param>
/// <param name="Message">The raw log message line.</param>
/// <param name="LogStreamName">The name of the log stream the event belongs to; empty when not reported.</param>
public sealed record LambdaLogEvent(
    string Timestamp,
    string Message,
    string LogStreamName);
