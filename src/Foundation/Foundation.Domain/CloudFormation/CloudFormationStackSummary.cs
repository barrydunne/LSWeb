namespace Foundation.Domain.CloudFormation;

/// <summary>
/// A concise view of a CloudFormation stack as it appears in a list.
/// </summary>
/// <param name="StackName">The stack name.</param>
/// <param name="StackId">The Amazon Resource Name that uniquely identifies the stack.</param>
/// <param name="StackStatus">The stack status, for example <c>CREATE_COMPLETE</c> or <c>UPDATE_COMPLETE</c>.</param>
/// <param name="Description">The template description, where one was provided.</param>
/// <param name="CreationTime">The moment the stack was created.</param>
/// <param name="LastUpdatedTime">The moment the stack was last updated, or <see langword="null"/> when it has not been updated.</param>
public sealed record CloudFormationStackSummary(
    string StackName,
    string StackId,
    string StackStatus,
    string? Description,
    DateTimeOffset CreationTime,
    DateTimeOffset? LastUpdatedTime);
