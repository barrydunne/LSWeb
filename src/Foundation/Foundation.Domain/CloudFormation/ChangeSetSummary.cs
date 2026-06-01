namespace Foundation.Domain.CloudFormation;

/// <summary>
/// A summary of a CloudFormation change set pending against a stack.
/// </summary>
/// <param name="ChangeSetId">The Amazon Resource Name of the change set.</param>
/// <param name="ChangeSetName">The name of the change set.</param>
/// <param name="StackName">The name of the stack the change set targets.</param>
/// <param name="Status">The current status of the change set, for example <c>CREATE_COMPLETE</c>.</param>
/// <param name="StatusReason">The reason for the current status, where one was provided.</param>
/// <param name="ExecutionStatus">Whether the change set is available to execute.</param>
/// <param name="Description">The description recorded against the change set, where one was provided.</param>
/// <param name="CreationTime">The time the change set was created.</param>
public sealed record ChangeSetSummary(
    string ChangeSetId,
    string ChangeSetName,
    string StackName,
    string Status,
    string? StatusReason,
    string ExecutionStatus,
    string? Description,
    DateTime CreationTime);
