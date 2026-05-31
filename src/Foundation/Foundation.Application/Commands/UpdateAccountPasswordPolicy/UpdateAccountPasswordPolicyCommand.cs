using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateAccountPasswordPolicy;

/// <summary>
/// Create or replace the account password policy.
/// </summary>
/// <param name="MinimumPasswordLength">The minimum number of characters a password must contain.</param>
/// <param name="RequireSymbols">Whether passwords must contain at least one symbol.</param>
/// <param name="RequireNumbers">Whether passwords must contain at least one number.</param>
/// <param name="RequireUppercaseCharacters">Whether passwords must contain at least one uppercase letter.</param>
/// <param name="RequireLowercaseCharacters">Whether passwords must contain at least one lowercase letter.</param>
/// <param name="AllowUsersToChangePassword">Whether users may change their own password.</param>
/// <param name="MaxPasswordAge">The number of days before a password expires, or <see langword="null"/> for no expiry.</param>
/// <param name="PasswordReusePrevention">The number of previous passwords that may not be reused, or <see langword="null"/> for none.</param>
/// <param name="HardExpiry">Whether users are prevented from setting a new password after expiry.</param>
public record UpdateAccountPasswordPolicyCommand(
    int MinimumPasswordLength,
    bool RequireSymbols,
    bool RequireNumbers,
    bool RequireUppercaseCharacters,
    bool RequireLowercaseCharacters,
    bool AllowUsersToChangePassword,
    int? MaxPasswordAge,
    int? PasswordReusePrevention,
    bool HardExpiry) : ICommand;
