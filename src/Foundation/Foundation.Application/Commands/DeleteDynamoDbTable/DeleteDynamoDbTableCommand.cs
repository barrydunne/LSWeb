using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteDynamoDbTable;

/// <summary>
/// Delete a DynamoDB table and all of the items it contains.
/// </summary>
/// <param name="TableName">The name of the table to delete.</param>
public record DeleteDynamoDbTableCommand(string TableName) : ICommand;
