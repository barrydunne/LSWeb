namespace Foundation.Domain.Iam;

/// <summary>
/// A concise view of an IAM managed policy as it appears in a policy list.
/// </summary>
/// <param name="PolicyName">The name of the policy.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the policy.</param>
/// <param name="PolicyId">The stable identifier assigned to the policy.</param>
/// <param name="Path">The path under which the policy is organised.</param>
/// <param name="DefaultVersionId">The identifier of the version that is currently the default.</param>
/// <param name="AttachmentCount">The number of principals the policy is attached to.</param>
/// <param name="IsAttachable">Whether the policy may be attached to a principal.</param>
/// <param name="Description">The description of the policy, if one was supplied.</param>
/// <param name="CreateDate">When the policy was created, if known.</param>
/// <param name="UpdateDate">When the policy was last updated, if known.</param>
public sealed record IamPolicy(
    string PolicyName,
    string Arn,
    string PolicyId,
    string Path,
    string DefaultVersionId,
    int AttachmentCount,
    bool IsAttachable,
    string? Description,
    DateTimeOffset? CreateDate,
    DateTimeOffset? UpdateDate);
