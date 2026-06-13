using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutDynamoDbItem;

/// <summary>
/// Create or replace a DynamoDB item from its full JSON representation, optionally guarded by a
/// condition expression.
/// </summary>
/// <param name="TableName">The name of the table to write to.</param>
/// <param name="ItemJson">The full item as a JSON document.</param>
/// <param name="ConditionExpression">An optional DynamoDB condition expression that must hold for the write to succeed, or <see langword="null"/> for an unconditional write.</param>
public record PutDynamoDbItemCommand(
    string TableName, string ItemJson, string? ConditionExpression) : ICommand;
