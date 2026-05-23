using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Sqs;

namespace Foundation.Application.Queries.ListSqsQueues;

/// <summary>
/// List the SQS queues available on the configured backend.
/// </summary>
public record ListSqsQueuesQuery : IQuery<ListSqsQueuesQueryResult>;

/// <summary>
/// The SQS queues returned by the backend.
/// </summary>
/// <param name="Queues">The queues, ordered as returned by the backend.</param>
public record ListSqsQueuesQueryResult(IReadOnlyList<SqsQueue> Queues);
