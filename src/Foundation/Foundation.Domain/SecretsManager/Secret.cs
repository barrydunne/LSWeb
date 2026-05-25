namespace Foundation.Domain.SecretsManager;

/// <summary>
/// A concise view of a Secrets Manager secret as it appears in a secret list.
/// </summary>
/// <param name="Name">The name of the secret.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the secret.</param>
/// <param name="Description">An optional human-readable description of the secret.</param>
/// <param name="CreatedDate">When the secret was created, if known.</param>
/// <param name="LastChangedDate">When the secret's metadata or value last changed, if known.</param>
public sealed record Secret(
    string Name,
    string Arn,
    string? Description,
    DateTimeOffset? CreatedDate,
    DateTimeOffset? LastChangedDate);
