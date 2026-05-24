using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.DynamoDb;

namespace Foundation.Application.Queries.GetDynamoDbTable;

/// <summary>
/// Describe a single DynamoDB table.
/// </summary>
/// <param name="TableName">The name of the table to describe.</param>
public record GetDynamoDbTableQuery(string TableName) : IQuery<GetDynamoDbTableQueryResult>;

/// <summary>
/// The DynamoDB table description returned by the backend.
/// </summary>
/// <param name="Table">The detailed description of the table.</param>
public record GetDynamoDbTableQueryResult(DynamoDbTableDetail Table);
