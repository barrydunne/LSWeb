using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutDynamoDbItem;

/// <summary>
/// Create or replace a DynamoDB item from its full JSON representation.
/// </summary>
/// <param name="TableName">The name of the table to write to.</param>
/// <param name="ItemJson">The full item as a JSON document.</param>
public record PutDynamoDbItemCommand(string TableName, string ItemJson) : ICommand;
