namespace Foundation.Domain.Iam;

/// <summary>
/// The full detail of an IAM managed policy, including its default version document and version history.
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
/// <param name="DefaultVersionDocument">The JSON document of the policy's default version.</param>
/// <param name="Versions">The full version history of the policy, ordered as returned by the backend.</param>
/// <param name="Tags">The key/value tags attached to the policy.</param>
public sealed record IamPolicyDetail(
    string PolicyName,
    string Arn,
    string PolicyId,
    string Path,
    string DefaultVersionId,
    int AttachmentCount,
    bool IsAttachable,
    string? Description,
    DateTimeOffset? CreateDate,
    DateTimeOffset? UpdateDate,
    string DefaultVersionDocument,
    IReadOnlyList<IamPolicyVersion> Versions,
    IReadOnlyList<IamTag> Tags);
