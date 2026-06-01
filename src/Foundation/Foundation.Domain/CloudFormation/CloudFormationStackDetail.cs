namespace Foundation.Domain.CloudFormation;

/// <summary>
/// The full details of a CloudFormation stack.
/// </summary>
/// <param name="StackName">The stack name.</param>
/// <param name="StackId">The Amazon Resource Name that uniquely identifies the stack.</param>
/// <param name="StackStatus">The stack status, for example <c>CREATE_COMPLETE</c> or <c>UPDATE_COMPLETE</c>.</param>
/// <param name="StackStatusReason">The reason for the current status, where one was provided.</param>
/// <param name="Description">The template description, where one was provided.</param>
/// <param name="CreationTime">The moment the stack was created.</param>
/// <param name="LastUpdatedTime">The moment the stack was last updated, or <see langword="null"/> when it has not been updated.</param>
/// <param name="Parameters">The input parameters the stack was deployed with.</param>
/// <param name="Outputs">The outputs the stack exposes.</param>
/// <param name="Tags">The tags applied to the stack.</param>
/// <param name="Capabilities">The capabilities acknowledged for the stack, for example <c>CAPABILITY_IAM</c>.</param>
public sealed record CloudFormationStackDetail(
    string StackName,
    string StackId,
    string StackStatus,
    string? StackStatusReason,
    string? Description,
    DateTimeOffset CreationTime,
    DateTimeOffset? LastUpdatedTime,
    IReadOnlyList<StackParameter> Parameters,
    IReadOnlyList<StackOutput> Outputs,
    IReadOnlyList<StackTag> Tags,
    IReadOnlyList<string> Capabilities);
