using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.DynamoDb;

namespace Foundation.Application.Queries.ExecuteDynamoDbBatchGet;

/// <summary>
/// Read a set of items by their primary keys in a single DynamoDB batch get operation.
/// </summary>
/// <param name="Keys">The primary keys to read.</param>
public record ExecuteDynamoDbBatchGetQuery(IReadOnlyList<DynamoDbBatchGetKey> Keys)
    : IQuery<ExecuteDynamoDbBatchGetQueryResult>;

/// <summary>
/// The outcome of a batch get operation.
/// </summary>
/// <param name="Result">The number of keys submitted and the items found.</param>
public record ExecuteDynamoDbBatchGetQueryResult(DynamoDbBatchGetResult Result);
