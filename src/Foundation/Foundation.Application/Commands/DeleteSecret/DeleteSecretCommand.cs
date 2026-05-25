using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteSecret;

/// <summary>
/// Delete a Secrets Manager secret and all of the versions it contains.
/// </summary>
/// <param name="SecretId">The name or ARN of the secret to delete.</param>
public record DeleteSecretCommand(string SecretId) : ICommand;
