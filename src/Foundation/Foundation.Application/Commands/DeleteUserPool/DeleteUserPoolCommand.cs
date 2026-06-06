using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteUserPool;

/// <summary>
/// Delete an Amazon Cognito user pool. This action cannot be undone.
/// </summary>
/// <param name="Id">The unique identifier of the user pool to delete.</param>
public record DeleteUserPoolCommand(string Id) : ICommand;
