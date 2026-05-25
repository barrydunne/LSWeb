using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetSecretValue;

/// <summary>
/// Get the current value of a Secrets Manager secret, masking the value unless a guarded reveal is
/// requested and permitted by the host.
/// </summary>
/// <param name="SecretId">The name or ARN of the secret to read.</param>
/// <param name="Reveal">Whether the caller has explicitly requested the unmasked value.</param>
public record GetSecretValueQuery(string SecretId, bool Reveal) : IQuery<GetSecretValueQueryResult>;

/// <summary>
/// The current value of a Secrets Manager secret, with the value masked as required.
/// </summary>
/// <param name="Name">The name of the secret.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the secret.</param>
/// <param name="VersionId">The identifier of the version that produced the value, if known.</param>
/// <param name="Value">The value to display; masked unless a reveal was both requested and permitted.</param>
/// <param name="RevealAllowed">Whether the host permits the value to be revealed.</param>
public record GetSecretValueQueryResult(
    string Name,
    string Arn,
    string? VersionId,
    string Value,
    bool RevealAllowed);
