using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteLogGroup;

/// <summary>
/// Delete a CloudWatch log group and all of the streams and events it contains.
/// </summary>
/// <param name="LogGroupName">The name of the log group to delete.</param>
public record DeleteLogGroupCommand(string LogGroupName) : ICommand;
