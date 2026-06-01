namespace Foundation.Domain.CloudFormation;

/// <summary>
/// A single chronological event recorded against a CloudFormation stack.
/// </summary>
/// <param name="EventId">The unique id of the event.</param>
/// <param name="Timestamp">The time the event occurred.</param>
/// <param name="LogicalResourceId">The logical id of the resource the event relates to.</param>
/// <param name="PhysicalResourceId">The physical id of the resource, where one exists.</param>
/// <param name="ResourceType">The CloudFormation resource type, for example <c>AWS::SQS::Queue</c>.</param>
/// <param name="ResourceStatus">The resource status the event records.</param>
/// <param name="ResourceStatusReason">The reason for the status, where one was provided.</param>
public sealed record StackEvent(
    string EventId,
    DateTime Timestamp,
    string LogicalResourceId,
    string? PhysicalResourceId,
    string ResourceType,
    string ResourceStatus,
    string? ResourceStatusReason);
