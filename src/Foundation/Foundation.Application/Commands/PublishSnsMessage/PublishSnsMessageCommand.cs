using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PublishSnsMessage;

/// <summary>
/// Publish a message to an SNS topic, optionally with a subject and custom string attributes.
/// </summary>
/// <param name="TopicArn">The Amazon Resource Name of the topic to publish to.</param>
/// <param name="Subject">The optional subject; ignored when null or empty.</param>
/// <param name="Message">The message body.</param>
/// <param name="MessageAttributes">Custom string message attributes to attach to the message.</param>
public record PublishSnsMessageCommand(
    string TopicArn,
    string? Subject,
    string Message,
    IReadOnlyDictionary<string, string> MessageAttributes) : ICommand;
