namespace Foundation.Domain.DynamoDb;

/// <summary>
/// The definition of a global secondary index (GSI) to add to an existing table.
/// </summary>
/// <param name="TableName">The name of the table to add the index to.</param>
/// <param name="IndexName">The name of the index to create.</param>
/// <param name="PartitionKeyName">The name of the index partition (<c>HASH</c>) key attribute.</param>
/// <param name="PartitionKeyType">The scalar type of the partition key, one of <c>S</c>, <c>N</c>, or <c>B</c>.</param>
/// <param name="SortKeyName">The name of the optional sort (<c>RANGE</c>) key attribute, or <see langword="null"/> for a hash-only index.</param>
/// <param name="SortKeyType">The scalar type of the sort key, or <see langword="null"/> when there is no sort key.</param>
/// <param name="ProjectionType">The projection type, one of <c>ALL</c> or <c>KEYS_ONLY</c>.</param>
public sealed record DynamoDbIndexSpecification(
    string TableName,
    string IndexName,
    string PartitionKeyName,
    string PartitionKeyType,
    string? SortKeyName,
    string? SortKeyType,
    string ProjectionType);
