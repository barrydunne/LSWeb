namespace Foundation.Domain.Cognito;

/// <summary>
/// A concise view of an Amazon Cognito user as it appears in a list.
/// </summary>
/// <param name="Username">The unique username of the user within the pool.</param>
/// <param name="Status">The account status of the user (for example <c>CONFIRMED</c> or <c>FORCE_CHANGE_PASSWORD</c>).</param>
/// <param name="Enabled">Whether the user account is enabled and able to sign in.</param>
/// <param name="CreatedDate">The moment the user was created, if reported.</param>
public sealed record CognitoUserSummary(
    string Username,
    string Status,
    bool Enabled,
    DateTimeOffset? CreatedDate);
