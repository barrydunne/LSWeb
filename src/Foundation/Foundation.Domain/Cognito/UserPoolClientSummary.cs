namespace Foundation.Domain.Cognito;

/// <summary>
/// A concise view of an Amazon Cognito user pool app client as it appears in a client list. The
/// list view does not include the client's full configuration; that is read from the client detail.
/// </summary>
/// <param name="ClientId">The unique identifier of the app client.</param>
/// <param name="ClientName">The human-readable name of the app client.</param>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
public sealed record UserPoolClientSummary(
    string ClientId,
    string ClientName,
    string UserPoolId);
