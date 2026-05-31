namespace Foundation.Domain.Iam;

/// <summary>
/// A concise view of an IAM user as it appears in a user list.
/// </summary>
/// <param name="UserName">The name of the user.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the user.</param>
/// <param name="UserId">The stable identifier assigned to the user.</param>
/// <param name="Path">The path under which the user is organised.</param>
/// <param name="CreateDate">When the user was created, if known.</param>
public sealed record IamUser(
    string UserName,
    string Arn,
    string UserId,
    string Path,
    DateTimeOffset? CreateDate);
