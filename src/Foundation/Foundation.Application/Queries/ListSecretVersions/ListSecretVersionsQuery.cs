using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.SecretsManager;

namespace Foundation.Application.Queries.ListSecretVersions;

/// <summary>
/// List the versions held for a Secrets Manager secret along with their staging labels.
/// </summary>
/// <param name="SecretId">The name or ARN of the secret to read.</param>
public record ListSecretVersionsQuery(string SecretId) : IQuery<ListSecretVersionsQueryResult>;

/// <summary>
/// The versions held for a Secrets Manager secret.
/// </summary>
/// <param name="Name">The name of the secret.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the secret.</param>
/// <param name="Versions">The versions held for the secret, ordered as returned by the backend.</param>
public record ListSecretVersionsQueryResult(
    string Name,
    string Arn,
    IReadOnlyList<SecretVersion> Versions);
