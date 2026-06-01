namespace Foundation.Domain.CloudFormation;

/// <summary>
/// A resource a CloudFormation stack manages.
/// </summary>
/// <param name="LogicalResourceId">The logical id the template assigns to the resource.</param>
/// <param name="PhysicalResourceId">The physical id of the provisioned resource, where one exists.</param>
/// <param name="ResourceType">The CloudFormation resource type, for example <c>AWS::SQS::Queue</c>.</param>
/// <param name="ResourceStatus">The current status of the resource.</param>
/// <param name="ResourceStatusReason">The reason for the current status, where one was provided.</param>
/// <param name="LastUpdatedTime">The time the resource status was last updated.</param>
public sealed record StackResource(
    string LogicalResourceId,
    string? PhysicalResourceId,
    string ResourceType,
    string ResourceStatus,
    string? ResourceStatusReason,
    DateTime LastUpdatedTime);
