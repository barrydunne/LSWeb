namespace Foundation.Domain.Iam;

/// <summary>
/// The account password policy that governs IAM user passwords on the backend.
/// </summary>
/// <param name="MinimumPasswordLength">The minimum number of characters a password must contain.</param>
/// <param name="RequireSymbols">Whether passwords must contain at least one symbol.</param>
/// <param name="RequireNumbers">Whether passwords must contain at least one number.</param>
/// <param name="RequireUppercaseCharacters">Whether passwords must contain at least one uppercase letter.</param>
/// <param name="RequireLowercaseCharacters">Whether passwords must contain at least one lowercase letter.</param>
/// <param name="AllowUsersToChangePassword">Whether users may change their own password.</param>
/// <param name="ExpirePasswords">Whether passwords expire after <paramref name="MaxPasswordAge"/> days.</param>
/// <param name="MaxPasswordAge">The number of days before a password expires, if expiry is enabled.</param>
/// <param name="PasswordReusePrevention">The number of previous passwords that may not be reused, if set.</param>
/// <param name="HardExpiry">Whether users are prevented from setting a new password after expiry.</param>
public sealed record IamPasswordPolicy(
    int MinimumPasswordLength,
    bool RequireSymbols,
    bool RequireNumbers,
    bool RequireUppercaseCharacters,
    bool RequireLowercaseCharacters,
    bool AllowUsersToChangePassword,
    bool ExpirePasswords,
    int? MaxPasswordAge,
    int? PasswordReusePrevention,
    bool HardExpiry);
