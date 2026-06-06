namespace Foundation.Domain.Cognito;

/// <summary>
/// A concise view of an Amazon Cognito user pool as it appears in a user pool list. The list view
/// does not include the pool's full configuration; that is read from the user pool detail.
/// </summary>
/// <param name="Id">The unique identifier of the user pool.</param>
/// <param name="Name">The human-readable name of the user pool.</param>
/// <param name="CreationDate">The moment the user pool was created, if reported.</param>
public sealed record UserPoolSummary(
    string Id,
    string Name,
    DateTimeOffset? CreationDate);
