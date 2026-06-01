namespace Foundation.Domain.CloudFormation;

/// <summary>
/// A single resource change a CloudFormation change set would make when executed.
/// </summary>
/// <param name="Action">The action the change applies, for example <c>Add</c>, <c>Modify</c>, or <c>Remove</c>.</param>
/// <param name="LogicalResourceId">The logical id the template assigns to the resource.</param>
/// <param name="PhysicalResourceId">The physical id of the affected resource, where one exists.</param>
/// <param name="ResourceType">The CloudFormation resource type, for example <c>AWS::SQS::Queue</c>.</param>
/// <param name="Replacement">Whether applying the change replaces the resource, where the backend reports it.</param>
public sealed record ResourceChange(
    string Action,
    string LogicalResourceId,
    string? PhysicalResourceId,
    string ResourceType,
    string? Replacement);
