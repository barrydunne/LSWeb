namespace Foundation.Domain.DynamoDb;

/// <summary>
/// A single DynamoDB item rendered as a JSON document.
/// </summary>
/// <param name="Json">The item as a JSON document.</param>
public sealed record DynamoDbItem(string Json);

/// <summary>
/// A bounded page of DynamoDB items returned by a scan.
/// </summary>
/// <param name="Items">The items in the page, each rendered as a JSON document.</param>
/// <param name="Truncated">Whether more items exist beyond this page.</param>
public sealed record DynamoDbItemPage(IReadOnlyList<DynamoDbItem> Items, bool Truncated);
