using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.DynamoDb;

namespace Foundation.Application.Queries.GetDynamoDbItem;

/// <summary>
/// Read a single DynamoDB item by its primary key.
/// </summary>
/// <param name="TableName">The name of the table to read from.</param>
/// <param name="KeyJson">The primary key as a JSON document containing the key attributes.</param>
public record GetDynamoDbItemQuery(string TableName, string KeyJson) : IQuery<GetDynamoDbItemQueryResult>;

/// <summary>
/// The DynamoDB item returned by the backend.
/// </summary>
/// <param name="Item">The item as a JSON document.</param>
public record GetDynamoDbItemQueryResult(DynamoDbItem Item);
