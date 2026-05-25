using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.SecretsManager;

namespace Foundation.Application.SecretsManager;

/// <summary>
/// Provides access to AWS Secrets Manager: listing the secrets on the configured backend, creating
/// a new secret, and deleting an existing one.
/// </summary>
public interface ISecretsManagerClient
{
    /// <summary>
    /// Lists the secrets available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The secrets, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<Secret>>> ListSecretsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new secret with the supplied name, description, and value.
    /// </summary>
    /// <param name="specification">The secret to create, including its name and value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the secret could not be created.</returns>
    Task<Result> CreateSecretAsync(SecretSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a secret and all of the versions it contains.
    /// </summary>
    /// <param name="secretId">The name or ARN of the secret to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the secret is missing or the backend could not be reached.</returns>
    Task<Result> DeleteSecretAsync(string secretId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current value of a secret.
    /// </summary>
    /// <param name="secretId">The name or ARN of the secret to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The secret value, or an error if the secret is missing or the backend could not be reached.</returns>
    Task<Result<SecretValue>> GetSecretValueAsync(string secretId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a new value against an existing secret, creating a new version.
    /// </summary>
    /// <param name="specification">The secret to update, including the new value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the secret is missing or the backend could not be reached.</returns>
    Task<Result> PutSecretValueAsync(SecretValueSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the versions held for a secret along with the staging labels attached to each version.
    /// </summary>
    /// <param name="secretId">The name or ARN of the secret to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The secret versions, or an error if the secret is missing or the backend could not be reached.</returns>
    Task<Result<SecretVersionList>> ListSecretVersionsAsync(string secretId, CancellationToken cancellationToken);
}
