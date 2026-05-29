using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Sqs;

namespace Foundation.Application.Queries.ListSqsConsumerLambdas;

/// <summary>
/// List the Lambda functions that consume a queue, detected from each function's event source
/// mappings.
/// </summary>
/// <param name="QueueName">The name of the queue to inspect.</param>
public record ListSqsConsumerLambdasQuery(string QueueName) : IQuery<ListSqsConsumerLambdasQueryResult>;

/// <summary>
/// The Lambda functions that consume a queue.
/// </summary>
/// <param name="Lambdas">The consuming functions, ordered by function name.</param>
public record ListSqsConsumerLambdasQueryResult(IReadOnlyList<SqsConsumerLambda> Lambdas);
