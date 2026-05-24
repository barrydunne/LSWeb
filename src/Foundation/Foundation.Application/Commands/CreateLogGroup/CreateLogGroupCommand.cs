using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateLogGroup;

/// <summary>
/// Create a new, empty CloudWatch log group.
/// </summary>
/// <param name="LogGroupName">The name of the log group to create.</param>
public record CreateLogGroupCommand(string LogGroupName) : ICommand;
