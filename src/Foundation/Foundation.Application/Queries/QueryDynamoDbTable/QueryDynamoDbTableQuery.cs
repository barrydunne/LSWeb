using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.DynamoDb;

namespace Foundation.Application.Queries.QueryDynamoDbTable;

/// <summary>
/// Query or scan a DynamoDB table or secondary index for a bounded page of items.
/// </summary>
/// <param name="Request">The query specification, including key conditions, filters, index, limit, and pagination token.</param>
public record QueryDynamoDbTableQuery(DynamoDbQueryRequest Request) : IQuery<QueryDynamoDbTableQueryResult>;

/// <summary>
/// The page of DynamoDB items returned by a query or scan.
/// </summary>
/// <param name="Page">The items and pagination token returned by the backend.</param>
public record QueryDynamoDbTableQueryResult(DynamoDbQueryResult Page);
