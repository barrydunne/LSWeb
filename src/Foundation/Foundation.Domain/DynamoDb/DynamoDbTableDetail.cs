namespace Foundation.Domain.DynamoDb;

/// <summary>
/// A single attribute that participates in a table or index key schema.
/// </summary>
/// <param name="AttributeName">The name of the key attribute.</param>
/// <param name="KeyType">The role of the attribute in the key, for example <c>HASH</c> or <c>RANGE</c>.</param>
public sealed record DynamoDbKeyElement(string AttributeName, string KeyType);

/// <summary>
/// The definition of an attribute that participates in a key, including its scalar type.
/// </summary>
/// <param name="AttributeName">The name of the attribute.</param>
/// <param name="AttributeType">The scalar attribute type, for example <c>S</c>, <c>N</c>, or <c>B</c>.</param>
public sealed record DynamoDbAttributeDefinition(string AttributeName, string AttributeType);

/// <summary>
/// A secondary index defined on a table.
/// </summary>
/// <param name="Name">The name of the index.</param>
/// <param name="Status">The current status of the index, if reported by the backend.</param>
/// <param name="KeySchema">The key attributes that make up the index, in key order.</param>
public sealed record DynamoDbSecondaryIndex(
    string Name,
    string? Status,
    IReadOnlyList<DynamoDbKeyElement> KeySchema);

/// <summary>
/// The detailed description of a DynamoDB table, including its key schema, throughput, status, and
/// secondary indexes.
/// </summary>
/// <param name="Name">The name of the table.</param>
/// <param name="Arn">The fully-qualified ARN of the table.</param>
/// <param name="Status">The current status of the table, for example <c>ACTIVE</c>.</param>
/// <param name="ItemCount">The approximate number of items in the table.</param>
/// <param name="TableSizeBytes">The approximate size of the table in bytes.</param>
/// <param name="BillingMode">The billing mode of the table, if reported by the backend.</param>
/// <param name="ReadCapacityUnits">The provisioned read capacity units, or <see langword="null"/> for on-demand tables.</param>
/// <param name="WriteCapacityUnits">The provisioned write capacity units, or <see langword="null"/> for on-demand tables.</param>
/// <param name="CreatedAt">The time the table was created, if reported by the backend.</param>
/// <param name="KeySchema">The primary key attributes, in key order.</param>
/// <param name="Attributes">The attribute definitions referenced by the key schema and indexes.</param>
/// <param name="GlobalSecondaryIndexes">The global secondary indexes defined on the table.</param>
/// <param name="LocalSecondaryIndexes">The local secondary indexes defined on the table.</param>
/// <param name="StreamEnabled">Whether a DynamoDB Stream is enabled on the table.</param>
/// <param name="StreamViewType">The view type of the stream, for example <c>NEW_AND_OLD_IMAGES</c>, if reported by the backend.</param>
/// <param name="LatestStreamArn">The ARN of the table's latest DynamoDB Stream, or <see langword="null"/> when no stream is active.</param>
/// <param name="TtlStatus">The time-to-live status, for example <c>ENABLED</c>, <c>DISABLED</c>, <c>ENABLING</c>, or <c>DISABLING</c>, if reported by the backend.</param>
/// <param name="TtlAttributeName">The attribute used as the time-to-live expiry timestamp, or <see langword="null"/> when TTL is not configured.</param>
public sealed record DynamoDbTableDetail(
    string Name,
    string Arn,
    string Status,
    long ItemCount,
    long TableSizeBytes,
    string? BillingMode,
    long? ReadCapacityUnits,
    long? WriteCapacityUnits,
    DateTimeOffset? CreatedAt,
    IReadOnlyList<DynamoDbKeyElement> KeySchema,
    IReadOnlyList<DynamoDbAttributeDefinition> Attributes,
    IReadOnlyList<DynamoDbSecondaryIndex> GlobalSecondaryIndexes,
    IReadOnlyList<DynamoDbSecondaryIndex> LocalSecondaryIndexes,
    bool StreamEnabled,
    string? StreamViewType,
    string? LatestStreamArn,
    string? TtlStatus,
    string? TtlAttributeName);
