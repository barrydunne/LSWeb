namespace Foundation.Domain.DynamoDb;

/// <summary>
/// A single write action that participates in an atomic DynamoDB transaction.
/// </summary>
/// <param name="Operation">The action to perform, one of <c>Put</c> or <c>Delete</c>.</param>
/// <param name="TableName">The name of the table the action targets.</param>
/// <param name="Json">
/// The action payload as a JSON document: the full item for a <c>Put</c>, or the primary key for a
/// <c>Delete</c>.
/// </param>
public sealed record DynamoDbTransactionAction(string Operation, string TableName, string Json);
