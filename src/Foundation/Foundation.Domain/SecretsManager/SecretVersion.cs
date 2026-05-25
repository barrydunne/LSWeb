namespace Foundation.Domain.SecretsManager;

/// <summary>
/// The versions held for a Secrets Manager secret along with the secret's identity.
/// </summary>
/// <param name="Name">The name of the secret.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the secret.</param>
/// <param name="Versions">The versions held for the secret, ordered as returned by the backend.</param>
public sealed record SecretVersionList(string Name, string Arn, IReadOnlyList<SecretVersion> Versions);

/// <summary>
/// A single version of a Secrets Manager secret along with the staging labels attached to it.
/// </summary>
/// <param name="VersionId">The identifier of the version.</param>
/// <param name="Stages">The staging labels attached to the version, such as AWSCURRENT or AWSPREVIOUS.</param>
/// <param name="CreatedDate">When the version was created, if known.</param>
/// <param name="LastAccessedDate">When the version was last accessed, if known.</param>
public sealed record SecretVersion(
    string VersionId,
    IReadOnlyList<string> Stages,
    DateTimeOffset? CreatedDate,
    DateTimeOffset? LastAccessedDate);
