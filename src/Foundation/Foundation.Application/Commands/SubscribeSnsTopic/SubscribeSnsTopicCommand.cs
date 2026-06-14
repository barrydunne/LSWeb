using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SubscribeSnsTopic;

/// <summary>
/// Subscribe an endpoint to an SNS topic using the supplied protocol.
/// </summary>
/// <param name="TopicArn">The Amazon Resource Name of the topic to subscribe to.</param>
/// <param name="Protocol">The delivery protocol, for example <c>sqs</c>, <c>lambda</c>, or <c>email</c>.</param>
/// <param name="Endpoint">The endpoint to deliver to.</param>
public record SubscribeSnsTopicCommand(string TopicArn, string Protocol, string Endpoint) : ICommand;
