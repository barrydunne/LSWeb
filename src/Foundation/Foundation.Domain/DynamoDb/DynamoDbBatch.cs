namespace Foundation.Domain.DynamoDb;

/// <summary>
/// A single write request in a non-atomic batch write operation.
/// </summary>
/// <param name="Operation">The action to perform, one of <c>Put</c> or <c>Delete</c>.</param>
/// <param name="TableName">The name of the table the request targets.</param>
/// <param name="Json">The full item for a <c>Put</c>, or the primary key for a <c>Delete</c>, as a JSON document.</param>
public sealed record DynamoDbBatchWriteItem(string Operation, string TableName, string Json);

/// <summary>
/// The outcome of a batch write operation, including any requests the backend could not process.
/// </summary>
/// <param name="Requested">The number of write requests submitted.</param>
/// <param name="UnprocessedItems">The requests the backend could not process, each rendered as a JSON document.</param>
public sealed record DynamoDbBatchWriteResult(int Requested, IReadOnlyList<string> UnprocessedItems);

/// <summary>
/// A single key in a batch get operation.
/// </summary>
/// <param name="TableName">The name of the table to read from.</param>
/// <param name="Json">The primary key as a JSON document containing the key attributes.</param>
public sealed record DynamoDbBatchGetKey(string TableName, string Json);

/// <summary>
/// The outcome of a batch get operation.
/// </summary>
/// <param name="Requested">The number of keys submitted.</param>
/// <param name="Items">The items found, each rendered as a JSON document.</param>
public sealed record DynamoDbBatchGetResult(int Requested, IReadOnlyList<DynamoDbItem> Items);
