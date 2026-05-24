using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteDynamoDbItem;

/// <summary>
/// Delete a single DynamoDB item by its primary key.
/// </summary>
/// <param name="TableName">The name of the table to delete from.</param>
/// <param name="KeyJson">The primary key as a JSON document containing the key attributes.</param>
public record DeleteDynamoDbItemCommand(string TableName, string KeyJson) : ICommand;
