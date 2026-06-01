using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteStack;

/// <summary>
/// Delete a CloudFormation stack by its name or Amazon Resource Name.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack to delete.</param>
public record DeleteStackCommand(string StackName) : ICommand;
