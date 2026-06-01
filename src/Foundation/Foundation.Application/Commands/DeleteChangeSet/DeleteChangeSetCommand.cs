using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteChangeSet;

/// <summary>
/// Delete a CloudFormation change set without applying its changes.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack the change set targets.</param>
/// <param name="ChangeSetName">The name or Amazon Resource Name of the change set to delete.</param>
public record DeleteChangeSetCommand(string StackName, string ChangeSetName) : ICommand;
