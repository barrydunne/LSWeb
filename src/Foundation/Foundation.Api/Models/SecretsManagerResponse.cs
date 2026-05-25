namespace Foundation.Api.Models;

/// <summary>
/// The Secrets Manager secrets available on the configured backend.
/// </summary>
/// <param name="Secrets">The secret summaries, ordered as returned by the backend.</param>
public sealed record SecretListResponse(IReadOnlyList<SecretSummaryResponse> Secrets);

/// <summary>
/// A concise view of a Secrets Manager secret as it appears in a secret list.
/// </summary>
/// <param name="Name">The name of the secret.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the secret.</param>
/// <param name="Description">An optional human-readable description of the secret.</param>
/// <param name="CreatedDate">When the secret was created, if known.</param>
/// <param name="LastChangedDate">When the secret's metadata or value last changed, if known.</param>
public sealed record SecretSummaryResponse(
    string Name,
    string Arn,
    string? Description,
    DateTimeOffset? CreatedDate,
    DateTimeOffset? LastChangedDate);

/// <summary>
/// The details required to create a new Secrets Manager secret.
/// </summary>
/// <param name="Name">The name of the secret to create.</param>
/// <param name="Description">An optional human-readable description of the secret.</param>
/// <param name="SecretString">The secret value to store.</param>
public sealed record SecretCreateRequest(
    string Name,
    string? Description,
    string SecretString);

/// <summary>
/// The current value of a Secrets Manager secret, with the value masked as required.
/// </summary>
/// <param name="Name">The name of the secret.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the secret.</param>
/// <param name="VersionId">The identifier of the version that produced the value, if known.</param>
/// <param name="Value">The value to display; masked unless a reveal was both requested and permitted.</param>
/// <param name="RevealAllowed">Whether the host permits the value to be revealed.</param>
public sealed record SecretValueResponse(
    string Name,
    string Arn,
    string? VersionId,
    string Value,
    bool RevealAllowed);

/// <summary>
/// A request to store a new value against an existing Secrets Manager secret.
/// </summary>
/// <param name="SecretString">The secret value to store.</param>
public sealed record SecretValueUpdateRequest(string SecretString);

/// <summary>
/// The versions held for a Secrets Manager secret along with their staging labels.
/// </summary>
/// <param name="Name">The name of the secret.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the secret.</param>
/// <param name="Versions">The versions held for the secret, ordered as returned by the backend.</param>
public sealed record SecretVersionListResponse(
    string Name,
    string Arn,
    IReadOnlyList<SecretVersionResponse> Versions);

/// <summary>
/// A single version of a Secrets Manager secret along with the staging labels attached to it.
/// </summary>
/// <param name="VersionId">The identifier of the version.</param>
/// <param name="Stages">The staging labels attached to the version, such as AWSCURRENT or AWSPREVIOUS.</param>
/// <param name="CreatedDate">When the version was created, if known.</param>
/// <param name="LastAccessedDate">When the version was last accessed, if known.</param>
public sealed record SecretVersionResponse(
    string VersionId,
    IReadOnlyList<string> Stages,
    DateTimeOffset? CreatedDate,
    DateTimeOffset? LastAccessedDate);
