using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteDynamoDbIndex;

/// <summary>
/// Delete a global secondary index (GSI) from an existing DynamoDB table.
/// </summary>
/// <param name="TableName">The name of the table that owns the index.</param>
/// <param name="IndexName">The name of the index to delete.</param>
public record DeleteDynamoDbIndexCommand(string TableName, string IndexName) : ICommand;
