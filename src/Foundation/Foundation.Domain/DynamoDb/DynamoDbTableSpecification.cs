namespace Foundation.Domain.DynamoDb;

/// <summary>
/// The specification for a new DynamoDB table: its name, primary key schema, and billing configuration.
/// </summary>
/// <param name="TableName">The name of the table to create.</param>
/// <param name="PartitionKeyName">The name of the partition (<c>HASH</c>) key attribute.</param>
/// <param name="PartitionKeyType">The scalar type of the partition key, one of <c>S</c>, <c>N</c>, or <c>B</c>.</param>
/// <param name="SortKeyName">The name of the optional sort (<c>RANGE</c>) key attribute, or <see langword="null"/> for a table with no sort key.</param>
/// <param name="SortKeyType">The scalar type of the sort key, or <see langword="null"/> when there is no sort key.</param>
/// <param name="BillingMode">The billing mode, one of <c>PAY_PER_REQUEST</c> or <c>PROVISIONED</c>.</param>
/// <param name="ReadCapacityUnits">The provisioned read capacity units, used only when <paramref name="BillingMode"/> is <c>PROVISIONED</c>.</param>
/// <param name="WriteCapacityUnits">The provisioned write capacity units, used only when <paramref name="BillingMode"/> is <c>PROVISIONED</c>.</param>
public sealed record DynamoDbTableSpecification(
    string TableName,
    string PartitionKeyName,
    string PartitionKeyType,
    string? SortKeyName,
    string? SortKeyType,
    string BillingMode,
    long? ReadCapacityUnits,
    long? WriteCapacityUnits);
