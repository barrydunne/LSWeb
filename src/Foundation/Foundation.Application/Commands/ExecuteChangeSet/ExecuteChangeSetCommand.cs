using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.ExecuteChangeSet;

/// <summary>
/// Execute a CloudFormation change set, applying its changes to the target stack.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack the change set targets.</param>
/// <param name="ChangeSetName">The name or Amazon Resource Name of the change set to execute.</param>
public record ExecuteChangeSetCommand(string StackName, string ChangeSetName) : ICommand;
