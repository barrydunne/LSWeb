namespace Foundation.Domain.SecretsManager;

/// <summary>
/// The resolved value of a Secrets Manager secret along with the metadata of the version that
/// produced it.
/// </summary>
/// <param name="Name">The name of the secret.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the secret.</param>
/// <param name="VersionId">The identifier of the version that produced the value, if known.</param>
/// <param name="SecretString">The stored secret value.</param>
public sealed record SecretValue(string Name, string Arn, string? VersionId, string SecretString);

/// <summary>
/// The details required to store a new value against an existing Secrets Manager secret.
/// </summary>
/// <param name="SecretId">The name or ARN of the secret to update.</param>
/// <param name="SecretString">The secret value to store.</param>
public sealed record SecretValueSpecification(string SecretId, string SecretString);
