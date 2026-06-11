using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateLogStream;

/// <summary>
/// Create a new, empty log stream within an existing CloudWatch log group.
/// </summary>
/// <param name="LogGroupName">The name of the log group the stream belongs to.</param>
/// <param name="LogStreamName">The name of the log stream to create.</param>
public record CreateLogStreamCommand(string LogGroupName, string LogStreamName) : ICommand;
