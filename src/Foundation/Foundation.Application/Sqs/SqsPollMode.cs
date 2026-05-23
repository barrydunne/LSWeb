namespace Foundation.Application.Sqs;

/// <summary>
/// Controls how a poll affects the visibility of the messages it returns.
/// </summary>
public enum SqsPollMode
{
    /// <summary>
    /// Peek the queue without consuming: messages are returned with a zero visibility timeout so
    /// they remain visible to other consumers immediately afterwards.
    /// </summary>
    Peek = 0,

    /// <summary>
    /// Consume from the queue: messages are returned with the queue's default visibility timeout so
    /// they become temporarily invisible to other consumers.
    /// </summary>
    Consume = 1,
}
