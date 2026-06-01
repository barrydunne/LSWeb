namespace Foundation.Domain.CloudFormation;

/// <summary>
/// The full detail of a CloudFormation change set, including the resource changes it would apply.
/// </summary>
/// <param name="ChangeSetName">The name of the change set.</param>
/// <param name="ChangeSetId">The Amazon Resource Name of the change set.</param>
/// <param name="StackName">The name of the stack the change set targets.</param>
/// <param name="StackId">The Amazon Resource Name of the stack the change set targets.</param>
/// <param name="Status">The current status of the change set, for example <c>CREATE_COMPLETE</c>.</param>
/// <param name="StatusReason">The reason for the current status, where one was provided.</param>
/// <param name="ExecutionStatus">Whether the change set is available to execute.</param>
/// <param name="Description">The description recorded against the change set, where one was provided.</param>
/// <param name="CreationTime">The time the change set was created.</param>
/// <param name="Parameters">The input parameters the change set carries.</param>
/// <param name="Capabilities">The capabilities the change set requires, such as CAPABILITY_IAM.</param>
/// <param name="Changes">The resource changes the change set would apply when executed.</param>
public sealed record ChangeSetDetail(
    string ChangeSetName,
    string ChangeSetId,
    string StackName,
    string StackId,
    string Status,
    string? StatusReason,
    string ExecutionStatus,
    string? Description,
    DateTime CreationTime,
    IReadOnlyList<StackParameter> Parameters,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<ResourceChange> Changes);
