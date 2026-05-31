namespace Foundation.Domain.Iam;

/// <summary>
/// A snapshot of account-wide IAM entity counts and quotas as reported by the backend.
/// </summary>
/// <param name="Entries">The summary values keyed by their AWS summary name, ordered by key.</param>
public sealed record IamAccountSummary(IReadOnlyDictionary<string, int> Entries);
