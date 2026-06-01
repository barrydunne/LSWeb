namespace Foundation.Domain.CloudFormation;

/// <summary>
/// The drift state of a single resource a CloudFormation stack manages, comparing the template's
/// expected configuration against the resource's actual configuration on the backend.
/// </summary>
/// <param name="LogicalResourceId">The logical id the template assigns to the resource.</param>
/// <param name="PhysicalResourceId">The physical id of the affected resource, where one exists.</param>
/// <param name="ResourceType">The CloudFormation resource type, for example <c>AWS::SQS::Queue</c>.</param>
/// <param name="DriftStatus">The drift status, for example <c>IN_SYNC</c>, <c>MODIFIED</c>, <c>DELETED</c>, or <c>NOT_CHECKED</c>.</param>
/// <param name="ExpectedProperties">The properties the template expects, as JSON, where the backend reports them.</param>
/// <param name="ActualProperties">The properties the resource actually has, as JSON, where the backend reports them.</param>
/// <param name="Timestamp">The time the drift was last evaluated.</param>
public sealed record StackResourceDrift(
    string LogicalResourceId,
    string? PhysicalResourceId,
    string ResourceType,
    string DriftStatus,
    string? ExpectedProperties,
    string? ActualProperties,
    DateTimeOffset Timestamp);
