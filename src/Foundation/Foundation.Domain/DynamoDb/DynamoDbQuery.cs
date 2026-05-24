namespace Foundation.Domain.DynamoDb;

/// <summary>
/// A single condition applied to a DynamoDB query or scan, such as a key condition or a filter.
/// </summary>
/// <param name="AttributeName">The name of the attribute the condition applies to.</param>
/// <param name="Operator">The comparison operator (for example "=", "&lt;", "begins_with", "between").</param>
/// <param name="ValueType">The DynamoDB scalar type of the value ("S", "N" or "BOOL").</param>
/// <param name="Value">The value to compare against, rendered as a string.</param>
/// <param name="SecondValue">The upper bound value, used only by the "between" operator.</param>
public sealed record DynamoDbCondition(
    string AttributeName,
    string Operator,
    string ValueType,
    string Value,
    string? SecondValue);

/// <summary>
/// A request to read items from a DynamoDB table or index using a query or a scan.
/// </summary>
/// <param name="TableName">The name of the table to read from.</param>
/// <param name="IndexName">The optional secondary index to read from instead of the base table.</param>
/// <param name="Scan">Whether to perform a scan; otherwise a query keyed on the partition key is performed.</param>
/// <param name="PartitionKey">The partition key condition, required when performing a query.</param>
/// <param name="SortKey">The optional sort key condition applied to a query.</param>
/// <param name="Filters">The filter conditions applied after the key conditions.</param>
/// <param name="Limit">The maximum number of items to return.</param>
/// <param name="StartToken">The pagination token returned by a previous call, or null for the first page.</param>
public sealed record DynamoDbQueryRequest(
    string TableName,
    string? IndexName,
    bool Scan,
    DynamoDbCondition? PartitionKey,
    DynamoDbCondition? SortKey,
    IReadOnlyList<DynamoDbCondition> Filters,
    int Limit,
    string? StartToken);

/// <summary>
/// The result of a DynamoDB query or scan.
/// </summary>
/// <param name="Items">The items returned, each rendered as a JSON document.</param>
/// <param name="NextToken">The pagination token to retrieve the next page, or null when no more items exist.</param>
public sealed record DynamoDbQueryResult(IReadOnlyList<DynamoDbItem> Items, string? NextToken);
