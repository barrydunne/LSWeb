namespace Foundation.Domain.DynamoDb;

/// <summary>
/// A request to run a PartiQL statement against DynamoDB.
/// </summary>
/// <param name="Statement">The PartiQL statement to execute.</param>
/// <param name="Limit">The maximum number of items to return for a read statement.</param>
/// <param name="NextToken">The pagination token returned by a previous call, or null for the first page.</param>
public sealed record DynamoDbStatementRequest(string Statement, int Limit, string? NextToken);

/// <summary>
/// The result of running a PartiQL statement.
/// </summary>
/// <param name="Items">The items returned, each rendered as a JSON document.</param>
/// <param name="NextToken">The pagination token to retrieve the next page, or null when no more items exist.</param>
public sealed record DynamoDbStatementResult(IReadOnlyList<DynamoDbItem> Items, string? NextToken);
