namespace Foundation.Domain.Iam;

/// <summary>
/// A concise view of an IAM role as it appears in a role list.
/// </summary>
/// <param name="RoleName">The name of the role.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the role.</param>
/// <param name="RoleId">The stable identifier assigned to the role.</param>
/// <param name="Path">The path under which the role is organised.</param>
/// <param name="CreateDate">When the role was created, if known.</param>
/// <param name="Description">The description of the role, if one was supplied.</param>
public sealed record IamRole(
    string RoleName,
    string Arn,
    string RoleId,
    string Path,
    DateTimeOffset? CreateDate,
    string? Description);
