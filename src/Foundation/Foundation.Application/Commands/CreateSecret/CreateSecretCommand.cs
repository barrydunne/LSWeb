using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateSecret;

/// <summary>
/// Create a new Secrets Manager secret with the supplied name, description, and value.
/// </summary>
/// <param name="Name">The name of the secret to create.</param>
/// <param name="Description">An optional human-readable description of the secret.</param>
/// <param name="SecretString">The secret value to store.</param>
public record CreateSecretCommand(
    string Name,
    string? Description,
    string SecretString) : ICommand;
