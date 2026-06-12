namespace Foundation.Domain.Cognito;

/// <summary>
/// The password complexity rules enforced by an Amazon Cognito user pool.
/// </summary>
/// <param name="MinimumLength">The minimum number of characters a password must contain.</param>
/// <param name="RequireUppercase">Whether passwords must contain at least one uppercase letter.</param>
/// <param name="RequireLowercase">Whether passwords must contain at least one lowercase letter.</param>
/// <param name="RequireNumbers">Whether passwords must contain at least one digit.</param>
/// <param name="RequireSymbols">Whether passwords must contain at least one symbol.</param>
public sealed record PasswordPolicy(
    int MinimumLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireNumbers,
    bool RequireSymbols);
