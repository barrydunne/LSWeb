using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.DynamoDb;

namespace Foundation.Application.Commands.ExecuteDynamoDbTransaction;

/// <summary>
/// Execute a set of write actions as a single atomic DynamoDB transaction.
/// </summary>
/// <param name="Actions">The write actions to apply atomically.</param>
public record ExecuteDynamoDbTransactionCommand(
    IReadOnlyList<DynamoDbTransactionAction> Actions) : ICommand;
