using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateSnsTopic;

/// <summary>
/// Create an SNS topic with the supplied name.
/// </summary>
/// <param name="Name">The name of the topic to create.</param>
public record CreateSnsTopicCommand(string Name) : ICommand;
