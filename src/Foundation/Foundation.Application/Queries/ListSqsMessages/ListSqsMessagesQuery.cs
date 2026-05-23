using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Application.Sqs;
using Foundation.Domain.Sqs;

namespace Foundation.Application.Queries.ListSqsMessages;

/// <summary>
/// Poll a queue for messages, either peeking (visibility-preserving) or consuming them.
/// </summary>
/// <param name="QueueName">The name of the queue to read from.</param>
/// <param name="Mode">Whether to peek (preserve visibility) or consume the messages.</param>
/// <param name="MaxMessages">The maximum number of messages to return; clamped to the backend limit.</param>
public record ListSqsMessagesQuery(string QueueName, SqsPollMode Mode, int MaxMessages)
    : IQuery<ListSqsMessagesQueryResult>;

/// <summary>
/// The SQS messages returned by a poll.
/// </summary>
/// <param name="Messages">The messages, ordered as returned by the backend.</param>
public record ListSqsMessagesQueryResult(IReadOnlyList<SqsMessage> Messages);
