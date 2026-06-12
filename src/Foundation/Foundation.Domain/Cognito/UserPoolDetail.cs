namespace Foundation.Domain.Cognito;

/// <summary>
/// The full configuration of an Amazon Cognito user pool, including its sign-in and verification
/// settings. The informational values describe the pool's identity and lifecycle and cannot be
/// changed directly.
/// </summary>
/// <param name="Id">The unique identifier of the user pool.</param>
/// <param name="Name">The human-readable name of the user pool.</param>
/// <param name="Arn">The Amazon Resource Name of the user pool, if reported.</param>
/// <param name="MfaConfiguration">The multi-factor authentication configuration (<c>OFF</c>, <c>ON</c> or <c>OPTIONAL</c>), if reported.</param>
/// <param name="EstimatedNumberOfUsers">The estimated number of users in the pool, if reported.</param>
/// <param name="UsernameAttributes">The attributes that may be used as a username when signing in (for example <c>email</c> or <c>phone_number</c>).</param>
/// <param name="AutoVerifiedAttributes">The attributes that Cognito automatically verifies (for example <c>email</c> or <c>phone_number</c>).</param>
/// <param name="CreationDate">The moment the user pool was created, if reported.</param>
/// <param name="LastModifiedDate">The moment the user pool was last modified, if reported.</param>
/// <param name="PasswordPolicy">The password complexity rules enforced by the pool, if reported.</param>
public sealed record UserPoolDetail(
    string Id,
    string Name,
    string? Arn,
    string? MfaConfiguration,
    int? EstimatedNumberOfUsers,
    IReadOnlyList<string> UsernameAttributes,
    IReadOnlyList<string> AutoVerifiedAttributes,
    DateTimeOffset? CreationDate,
    DateTimeOffset? LastModifiedDate,
    PasswordPolicy? PasswordPolicy);
