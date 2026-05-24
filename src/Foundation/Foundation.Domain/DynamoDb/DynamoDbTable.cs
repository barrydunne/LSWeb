namespace Foundation.Domain.DynamoDb;

/// <summary>
/// A concise view of a DynamoDB table as it appears in a table list.
/// </summary>
/// <param name="Name">The name of the table.</param>
public sealed record DynamoDbTable(string Name);
