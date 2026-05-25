using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.SecretsManager;

namespace Foundation.Application.Queries.ListSecrets;

/// <summary>
/// List the Secrets Manager secrets available on the configured backend.
/// </summary>
public record ListSecretsQuery() : IQuery<ListSecretsQueryResult>;

/// <summary>
/// The Secrets Manager secrets returned by the backend.
/// </summary>
/// <param name="Secrets">The secrets, ordered as returned by the backend.</param>
public record ListSecretsQueryResult(IReadOnlyList<Secret> Secrets);
