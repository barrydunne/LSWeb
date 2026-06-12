namespace Foundation.Domain.Cognito;

/// <summary>
/// The result of requesting bearer tokens for an Amazon Cognito app client, including the issued
/// tokens and the decoded claims of the identity token.
/// </summary>
/// <param name="AccessToken">The issued access token, if any.</param>
/// <param name="IdToken">The issued identity token, if any.</param>
/// <param name="RefreshToken">The issued refresh token, if any.</param>
/// <param name="TokenType">The type of the issued tokens (for example <c>Bearer</c>), if reported.</param>
/// <param name="ExpiresIn">The number of seconds until the access token expires, if reported.</param>
/// <param name="Claims">The claims decoded from the identity token.</param>
public sealed record TokenResult(
    string? AccessToken,
    string? IdToken,
    string? RefreshToken,
    string? TokenType,
    int? ExpiresIn,
    IReadOnlyList<CognitoUserAttributeEntry> Claims);
