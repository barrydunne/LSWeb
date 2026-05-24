using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.DynamoDb;

namespace Foundation.Application.Queries.ListDynamoDbTables;

/// <summary>
/// List the DynamoDB tables available on the configured backend.
/// </summary>
public record ListDynamoDbTablesQuery() : IQuery<ListDynamoDbTablesQueryResult>;

/// <summary>
/// The DynamoDB tables returned by the backend.
/// </summary>
/// <param name="Tables">The tables, ordered as returned by the backend.</param>
public record ListDynamoDbTablesQueryResult(IReadOnlyList<DynamoDbTable> Tables);
