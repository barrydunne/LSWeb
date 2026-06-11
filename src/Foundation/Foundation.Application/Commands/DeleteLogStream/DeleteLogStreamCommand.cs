using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteLogStream;

/// <summary>
/// Delete a CloudWatch log stream and all of the events it contains.
/// </summary>
/// <param name="LogGroupName">The name of the log group the stream belongs to.</param>
/// <param name="LogStreamName">The name of the log stream to delete.</param>
public record DeleteLogStreamCommand(string LogGroupName, string LogStreamName) : ICommand;
