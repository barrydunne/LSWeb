namespace Foundation.Domain.Sns;

/// <summary>
/// The details required to create an SNS topic.
/// </summary>
/// <param name="Name">The name of the topic to create.</param>
public sealed record SnsTopicSpecification(string Name);
