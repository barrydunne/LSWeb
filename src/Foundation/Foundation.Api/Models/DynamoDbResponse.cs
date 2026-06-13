namespace Foundation.Api.Models;

/// <summary>
/// The DynamoDB tables available on the configured backend.
/// </summary>
/// <param name="Tables">The table summaries, ordered as returned by the backend.</param>
public sealed record DynamoDbTableListResponse(IReadOnlyList<DynamoDbTableResponse> Tables);

/// <summary>
/// A concise view of a DynamoDB table as it appears in a table list.
/// </summary>
/// <param name="Name">The name of the table.</param>
public sealed record DynamoDbTableResponse(string Name);

/// <summary>
/// A single attribute that participates in a table or index key schema.
/// </summary>
/// <param name="AttributeName">The name of the key attribute.</param>
/// <param name="KeyType">The role of the attribute in the key, for example <c>HASH</c> or <c>RANGE</c>.</param>
public sealed record DynamoDbKeyAttributeResponse(string AttributeName, string KeyType);

/// <summary>
/// The definition of an attribute that participates in a key, including its scalar type.
/// </summary>
/// <param name="AttributeName">The name of the attribute.</param>
/// <param name="AttributeType">The scalar attribute type, for example <c>S</c>, <c>N</c>, or <c>B</c>.</param>
public sealed record DynamoDbAttributeResponse(string AttributeName, string AttributeType);

/// <summary>
/// A secondary index defined on a table.
/// </summary>
/// <param name="Name">The name of the index.</param>
/// <param name="Status">The current status of the index, if reported by the backend.</param>
/// <param name="KeySchema">The key attributes that make up the index, in key order.</param>
public sealed record DynamoDbSecondaryIndexResponse(
    string Name,
    string? Status,
    IReadOnlyList<DynamoDbKeyAttributeResponse> KeySchema);

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
/// <param name="TtlStatus">The time-to-live status, for example <c>ENABLED</c> or <c>DISABLED</c>, if reported by the backend.</param>
/// <param name="TtlAttributeName">The attribute used as the time-to-live expiry timestamp, or <see langword="null"/> when TTL is not configured.</param>
public sealed record DynamoDbTableDetailResponse(
    string Name,
    string Arn,
    string Status,
    long ItemCount,
    long TableSizeBytes,
    string? BillingMode,
    long? ReadCapacityUnits,
    long? WriteCapacityUnits,
    DateTimeOffset? CreatedAt,
    IReadOnlyList<DynamoDbKeyAttributeResponse> KeySchema,
    IReadOnlyList<DynamoDbAttributeResponse> Attributes,
    IReadOnlyList<DynamoDbSecondaryIndexResponse> GlobalSecondaryIndexes,
    IReadOnlyList<DynamoDbSecondaryIndexResponse> LocalSecondaryIndexes,
    bool StreamEnabled,
    string? StreamViewType,
    string? LatestStreamArn,
    string? TtlStatus,
    string? TtlAttributeName);

/// <summary>
/// The details of a DynamoDB table to create.
/// </summary>
/// <param name="TableName">The name of the table to create.</param>
/// <param name="PartitionKeyName">The name of the partition (<c>HASH</c>) key attribute.</param>
/// <param name="PartitionKeyType">The scalar type of the partition key, one of <c>S</c>, <c>N</c>, or <c>B</c>.</param>
/// <param name="SortKeyName">The name of the optional sort (<c>RANGE</c>) key attribute, or <see langword="null"/> for a table with no sort key.</param>
/// <param name="SortKeyType">The scalar type of the sort key, or <see langword="null"/> when there is no sort key.</param>
/// <param name="BillingMode">The billing mode, one of <c>PAY_PER_REQUEST</c> or <c>PROVISIONED</c>.</param>
/// <param name="ReadCapacityUnits">The provisioned read capacity units, used only when <paramref name="BillingMode"/> is <c>PROVISIONED</c>.</param>
/// <param name="WriteCapacityUnits">The provisioned write capacity units, used only when <paramref name="BillingMode"/> is <c>PROVISIONED</c>.</param>
public sealed record DynamoDbTableCreateRequest(
    string TableName,
    string PartitionKeyName,
    string PartitionKeyType,
    string? SortKeyName,
    string? SortKeyType,
    string BillingMode,
    long? ReadCapacityUnits,
    long? WriteCapacityUnits);

/// <summary>
/// A single DynamoDB item rendered as a JSON document.
/// </summary>
/// <param name="Json">The item as a JSON document.</param>
public sealed record DynamoDbItemResponse(string Json);

/// <summary>
/// A bounded page of DynamoDB items returned by a scan.
/// </summary>
/// <param name="Items">The items in the page, each rendered as a JSON document.</param>
/// <param name="Truncated">Whether more items exist beyond this page.</param>
public sealed record DynamoDbItemListResponse(IReadOnlyList<DynamoDbItemResponse> Items, bool Truncated);

/// <summary>
/// The body of a request to create or replace a DynamoDB item.
/// </summary>
/// <param name="Item">The full item as a JSON document.</param>
/// <param name="ConditionExpression">An optional DynamoDB condition expression that must hold for the write to succeed, or <see langword="null"/> for an unconditional write.</param>
public sealed record DynamoDbItemPutRequest(string Item, string? ConditionExpression);

/// <summary>
/// The body of a request to enable or disable time-to-live (TTL) on a DynamoDB table.
/// </summary>
/// <param name="Enabled">Whether TTL should be enabled.</param>
/// <param name="AttributeName">The attribute used as the TTL expiry timestamp.</param>
public sealed record DynamoDbTtlUpdateRequest(bool Enabled, string AttributeName);

/// <summary>
/// The body of a request to add a global secondary index (GSI) to a DynamoDB table.
/// </summary>
/// <param name="IndexName">The name of the index to create.</param>
/// <param name="PartitionKeyName">The name of the index partition (<c>HASH</c>) key attribute.</param>
/// <param name="PartitionKeyType">The scalar type of the partition key, one of <c>S</c>, <c>N</c>, or <c>B</c>.</param>
/// <param name="SortKeyName">The name of the optional sort (<c>RANGE</c>) key attribute, or <see langword="null"/> for a hash-only index.</param>
/// <param name="SortKeyType">The scalar type of the sort key, or <see langword="null"/> when there is no sort key.</param>
/// <param name="ProjectionType">The projection type, one of <c>ALL</c> or <c>KEYS_ONLY</c>.</param>
public sealed record DynamoDbIndexCreateRequest(
    string IndexName,
    string PartitionKeyName,
    string PartitionKeyType,
    string? SortKeyName,
    string? SortKeyType,
    string ProjectionType);

/// <summary>
/// A single condition applied to a DynamoDB query or scan, such as a key condition or a filter.
/// </summary>
/// <param name="AttributeName">The name of the attribute the condition applies to.</param>
/// <param name="Operator">The comparison operator, for example <c>=</c>, <c>begins_with</c>, or <c>between</c>.</param>
/// <param name="ValueType">The DynamoDB scalar type of the value, one of <c>S</c>, <c>N</c>, or <c>BOOL</c>.</param>
/// <param name="Value">The value to compare against, rendered as a string.</param>
/// <param name="SecondValue">The upper bound value, used only by the <c>between</c> operator.</param>
public sealed record DynamoDbQueryConditionRequest(
    string AttributeName,
    string Operator,
    string ValueType,
    string Value,
    string? SecondValue);

/// <summary>
/// The body of a request to query or scan a DynamoDB table or secondary index.
/// </summary>
/// <param name="IndexName">The optional secondary index to read from instead of the base table.</param>
/// <param name="Scan">Whether to perform a scan; otherwise a query keyed on the partition key is performed.</param>
/// <param name="PartitionKey">The partition key condition, required when performing a query.</param>
/// <param name="SortKey">The optional sort key condition applied to a query.</param>
/// <param name="Filters">The filter conditions applied after the key conditions.</param>
/// <param name="Limit">The maximum number of items to return; clamped to the range 1 to 100.</param>
/// <param name="StartToken">The pagination token returned by a previous call, or <see langword="null"/> for the first page.</param>
public sealed record DynamoDbQueryRequestBody(
    string? IndexName,
    bool Scan,
    DynamoDbQueryConditionRequest? PartitionKey,
    DynamoDbQueryConditionRequest? SortKey,
    IReadOnlyList<DynamoDbQueryConditionRequest>? Filters,
    int Limit,
    string? StartToken);

/// <summary>
/// A bounded page of DynamoDB items returned by a query or scan, with a pagination token.
/// </summary>
/// <param name="Items">The items in the page, each rendered as a JSON document.</param>
/// <param name="NextToken">The pagination token to retrieve the next page, or <see langword="null"/> when no more items exist.</param>
public sealed record DynamoDbQueryResultResponse(
    IReadOnlyList<DynamoDbItemResponse> Items,
    string? NextToken);

/// <summary>
/// The body of a request to run a PartiQL statement against DynamoDB.
/// </summary>
/// <param name="Statement">The PartiQL statement to execute.</param>
/// <param name="Limit">The maximum number of items to return for a read statement; clamped to the range 1 to 100.</param>
/// <param name="NextToken">The pagination token returned by a previous call, or <see langword="null"/> for the first page.</param>
public sealed record DynamoDbStatementRequestBody(
    string Statement,
    int Limit,
    string? NextToken);

/// <summary>
/// A single write action that participates in an atomic DynamoDB transaction.
/// </summary>
/// <param name="Operation">The action to perform, one of <c>Put</c> or <c>Delete</c>.</param>
/// <param name="TableName">The name of the table the action targets.</param>
/// <param name="Json">The action payload as a JSON document: the full item for a <c>Put</c>, or the primary key for a <c>Delete</c>.</param>
public sealed record DynamoDbTransactionActionRequest(string Operation, string TableName, string Json);

/// <summary>
/// The body of a request to execute a set of write actions as a single atomic DynamoDB transaction.
/// </summary>
/// <param name="Actions">The write actions to apply atomically.</param>
public sealed record DynamoDbTransactionRequestBody(IReadOnlyList<DynamoDbTransactionActionRequest> Actions);

/// <summary>
/// A single write request in a non-atomic DynamoDB batch write.
/// </summary>
/// <param name="Operation">The action to perform, one of <c>Put</c> or <c>Delete</c>.</param>
/// <param name="TableName">The name of the table the request targets.</param>
/// <param name="Json">The full item for a <c>Put</c>, or the primary key for a <c>Delete</c>, as a JSON document.</param>
public sealed record DynamoDbBatchWriteItemRequest(string Operation, string TableName, string Json);

/// <summary>
/// The body of a request to execute a non-atomic DynamoDB batch write.
/// </summary>
/// <param name="Items">The write requests to apply.</param>
public sealed record DynamoDbBatchWriteRequestBody(IReadOnlyList<DynamoDbBatchWriteItemRequest> Items);

/// <summary>
/// The outcome of a DynamoDB batch write.
/// </summary>
/// <param name="Requested">The number of write requests submitted.</param>
/// <param name="UnprocessedItems">The requests the backend could not process, each rendered as a JSON document.</param>
public sealed record DynamoDbBatchWriteResponse(int Requested, IReadOnlyList<string> UnprocessedItems);

/// <summary>
/// A single key in a DynamoDB batch get.
/// </summary>
/// <param name="TableName">The name of the table to read from.</param>
/// <param name="Json">The primary key as a JSON document containing the key attributes.</param>
public sealed record DynamoDbBatchGetKeyRequest(string TableName, string Json);

/// <summary>
/// The body of a request to execute a DynamoDB batch get.
/// </summary>
/// <param name="Keys">The primary keys to read.</param>
public sealed record DynamoDbBatchGetRequestBody(IReadOnlyList<DynamoDbBatchGetKeyRequest> Keys);

/// <summary>
/// The outcome of a DynamoDB batch get.
/// </summary>
/// <param name="Requested">The number of keys submitted.</param>
/// <param name="Items">The items found, each rendered as a JSON document.</param>
public sealed record DynamoDbBatchGetResponse(int Requested, IReadOnlyList<DynamoDbItemResponse> Items);

/// <summary>
/// The page of DynamoDB items returned by a PartiQL statement, with a pagination token.
/// </summary>
/// <param name="Items">The items in the page, each rendered as a JSON document.</param>
/// <param name="NextToken">The pagination token to retrieve the next page, or <see langword="null"/> when no more items exist.</param>
public sealed record DynamoDbStatementResultResponse(
    IReadOnlyList<DynamoDbItemResponse> Items,
    string? NextToken);
