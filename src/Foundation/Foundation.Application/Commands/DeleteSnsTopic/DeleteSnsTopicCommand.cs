using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteSnsTopic;

/// <summary>
/// Delete an SNS topic by its Amazon Resource Name. This is a destructive action that cannot be
/// undone.
/// </summary>
/// <param name="TopicArn">The Amazon Resource Name of the topic to delete.</param>
public record DeleteSnsTopicCommand(string TopicArn) : ICommand;
