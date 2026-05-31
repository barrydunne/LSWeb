namespace Foundation.Domain.Iam;

/// <summary>
/// A concise view of an IAM group as it appears in a group list.
/// </summary>
/// <param name="GroupName">The name of the group.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the group.</param>
/// <param name="GroupId">The stable identifier assigned to the group.</param>
/// <param name="Path">The path under which the group is organised.</param>
/// <param name="CreateDate">When the group was created, if known.</param>
public sealed record IamGroup(
    string GroupName,
    string Arn,
    string GroupId,
    string Path,
    DateTimeOffset? CreateDate);
