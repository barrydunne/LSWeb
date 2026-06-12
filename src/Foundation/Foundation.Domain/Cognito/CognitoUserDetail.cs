namespace Foundation.Domain.Cognito;

/// <summary>
/// The full configuration of an Amazon Cognito user, including its attributes and account status.
/// </summary>
/// <param name="Username">The unique username of the user within the pool.</param>
/// <param name="Status">The account status of the user (for example <c>CONFIRMED</c> or <c>FORCE_CHANGE_PASSWORD</c>).</param>
/// <param name="Enabled">Whether the user account is enabled and able to sign in.</param>
/// <param name="Attributes">The attributes recorded against the user.</param>
/// <param name="CreatedDate">The moment the user was created, if reported.</param>
/// <param name="LastModifiedDate">The moment the user was last modified, if reported.</param>
public sealed record CognitoUserDetail(
    string Username,
    string Status,
    bool Enabled,
    IReadOnlyList<CognitoUserAttributeEntry> Attributes,
    DateTimeOffset? CreatedDate,
    DateTimeOffset? LastModifiedDate);
