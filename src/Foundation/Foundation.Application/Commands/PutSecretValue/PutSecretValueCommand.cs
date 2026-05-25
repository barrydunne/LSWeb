using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutSecretValue;

/// <summary>
/// Store a new value against an existing Secrets Manager secret, creating a new version.
/// </summary>
/// <param name="SecretId">The name or ARN of the secret to update.</param>
/// <param name="SecretString">The secret value to store.</param>
public record PutSecretValueCommand(string SecretId, string SecretString) : ICommand;
