namespace Foundation.Domain.Iam;

/// <summary>
/// The full detail of an IAM group, including its members and the policies attached to it.
/// </summary>
/// <param name="GroupName">The name of the group.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the group.</param>
/// <param name="GroupId">The stable identifier assigned to the group.</param>
/// <param name="Path">The path under which the group is organised.</param>
/// <param name="CreateDate">When the group was created, if known.</param>
/// <param name="Members">The names of the users that belong to the group.</param>
/// <param name="AttachedPolicies">The managed policies attached to the group.</param>
/// <param name="InlinePolicies">The inline policies embedded in the group, with their documents.</param>
public sealed record IamGroupDetail(
    string GroupName,
    string Arn,
    string GroupId,
    string Path,
    DateTimeOffset? CreateDate,
    IReadOnlyList<string> Members,
    IReadOnlyList<IamAttachedPolicy> AttachedPolicies,
    IReadOnlyList<IamInlinePolicy> InlinePolicies);

/// <summary>
/// An inline policy embedded in an IAM principal, including its JSON document.
/// </summary>
/// <param name="PolicyName">The name of the inline policy.</param>
/// <param name="PolicyDocument">The JSON policy document.</param>
public sealed record IamInlinePolicy(string PolicyName, string PolicyDocument);
