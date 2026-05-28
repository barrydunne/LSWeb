namespace Foundation.Domain.Sns;

/// <summary>
/// A concise view of an SNS topic as it appears in a topic list.
/// </summary>
/// <param name="Name">The topic name derived from its Amazon Resource Name.</param>
/// <param name="TopicArn">The Amazon Resource Name that uniquely identifies the topic.</param>
public sealed record SnsTopic(
    string Name,
    string TopicArn);
