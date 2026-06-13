using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateDynamoDbTtl;

/// <summary>
/// Enable or disable time-to-live (TTL) on a DynamoDB table, nominating the attribute that holds the
/// expiry timestamp.
/// </summary>
/// <param name="TableName">The name of the table to configure.</param>
/// <param name="Enabled">Whether TTL should be enabled.</param>
/// <param name="AttributeName">The attribute used as the TTL expiry timestamp.</param>
public record UpdateDynamoDbTtlCommand(
    string TableName,
    bool Enabled,
    string AttributeName) : ICommand;
