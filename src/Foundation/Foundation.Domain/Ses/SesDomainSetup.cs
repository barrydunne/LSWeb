namespace Foundation.Domain.Ses;

/// <summary>
/// The domain verification and DKIM setup state for an SES domain identity, carrying the tokens the
/// operator must publish as DNS records along with the current verification statuses.
/// </summary>
/// <param name="Domain">The domain name.</param>
/// <param name="VerificationStatus">
/// The domain verification status, such as <c>Success</c>, <c>Pending</c>, <c>Failed</c>, or
/// <c>NotStarted</c>.
/// </param>
/// <param name="VerificationToken">
/// The token to publish as a <c>TXT</c> record at <c>_amazonses.&lt;domain&gt;</c>, or an empty
/// string when domain verification has not been initiated.
/// </param>
/// <param name="DkimVerificationStatus">
/// The DKIM verification status, such as <c>Success</c>, <c>Pending</c>, or <c>NotStarted</c>.
/// </param>
/// <param name="DkimTokens">
/// The DKIM tokens; each is published as a <c>CNAME</c> record at
/// <c>&lt;token&gt;._domainkey.&lt;domain&gt;</c> pointing at <c>&lt;token&gt;.dkim.amazonses.com</c>.
/// </param>
public sealed record SesDomainSetup(
    string Domain,
    string VerificationStatus,
    string VerificationToken,
    string DkimVerificationStatus,
    IReadOnlyList<string> DkimTokens);
