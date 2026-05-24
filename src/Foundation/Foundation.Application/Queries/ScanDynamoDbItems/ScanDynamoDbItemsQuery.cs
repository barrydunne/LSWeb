using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.DynamoDb;

namespace Foundation.Application.Queries.ScanDynamoDbItems;

/// <summary>
/// Scan a DynamoDB table for a bounded page of items.
/// </summary>
/// <param name="TableName">The name of the table to scan.</param>
/// <param name="Limit">The maximum number of items to return.</param>
public record ScanDynamoDbItemsQuery(string TableName, int Limit) : IQuery<ScanDynamoDbItemsQueryResult>;

/// <summary>
/// The page of DynamoDB items returned by a scan.
/// </summary>
/// <param name="Page">The items and pagination state returned by the backend.</param>
public record ScanDynamoDbItemsQueryResult(DynamoDbItemPage Page);
