namespace Foundation.Domain.SecretsManager;

/// <summary>
/// The details required to create a new Secrets Manager secret.
/// </summary>
/// <param name="Name">The name of the secret to create.</param>
/// <param name="Description">An optional human-readable description of the secret.</param>
/// <param name="SecretString">The secret value to store.</param>
public sealed record SecretSpecification(
    string Name,
    string? Description,
    string SecretString);
