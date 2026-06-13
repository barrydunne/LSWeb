using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.DynamoDb;

namespace Foundation.Application.Queries.ExecuteDynamoDbBatchWrite;

/// <summary>
/// Execute a set of put and delete requests as a non-atomic DynamoDB batch write.
/// </summary>
/// <param name="Items">The write requests to apply.</param>
public record ExecuteDynamoDbBatchWriteQuery(IReadOnlyList<DynamoDbBatchWriteItem> Items)
    : IQuery<ExecuteDynamoDbBatchWriteQueryResult>;

/// <summary>
/// The outcome of a batch write operation.
/// </summary>
/// <param name="Result">The number of requests submitted and any the backend could not process.</param>
public record ExecuteDynamoDbBatchWriteQueryResult(DynamoDbBatchWriteResult Result);
