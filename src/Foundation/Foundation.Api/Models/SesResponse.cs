namespace Foundation.Api.Models;

/// <summary>
/// The SES identities available on the backend.
/// </summary>
/// <param name="Identities">The identity summaries, ordered as returned by the backend.</param>
public sealed record SesIdentityListResponse(
    IReadOnlyList<SesIdentitySummaryResponse> Identities);

/// <summary>
/// A concise view of an SES identity (an email address or domain) as it appears in a list.
/// </summary>
/// <param name="Identity">The email address or domain name of the identity.</param>
/// <param name="IdentityType">The identity type, either <c>EmailAddress</c> or <c>Domain</c>.</param>
/// <param name="VerificationStatus">The verification status, such as <c>Success</c> or <c>Pending</c>.</param>
public sealed record SesIdentitySummaryResponse(
    string Identity,
    string IdentityType,
    string VerificationStatus);

/// <summary>
/// The verification detail for a single SES identity.
/// </summary>
/// <param name="Identity">The email address or domain name of the identity.</param>
/// <param name="IdentityType">The identity type, either <c>EmailAddress</c> or <c>Domain</c>.</param>
/// <param name="VerificationStatus">
/// The verification status, such as <c>Success</c>, <c>Pending</c>, <c>Failed</c>, or
/// <c>NotStarted</c>.
/// </param>
public sealed record SesIdentityDetailResponse(
    string Identity,
    string IdentityType,
    string VerificationStatus);

/// <summary>
/// The request body to start the verification of an SES email address identity.
/// </summary>
/// <param name="EmailAddress">The email address to verify.</param>
public sealed record SesVerifyEmailRequest(string EmailAddress);

/// <summary>
/// The request body to initiate the verification of an SES domain identity.
/// </summary>
/// <param name="Domain">The domain name to verify.</param>
public sealed record SesVerifyDomainRequest(string Domain);

/// <summary>
/// The domain verification and DKIM setup state for an SES domain identity.
/// </summary>
/// <param name="Domain">The domain name.</param>
/// <param name="VerificationStatus">The domain verification status.</param>
/// <param name="VerificationToken">
/// The token to publish as a <c>TXT</c> record at <c>_amazonses.&lt;domain&gt;</c>.
/// </param>
/// <param name="DkimVerificationStatus">The DKIM verification status.</param>
/// <param name="DkimTokens">The DKIM tokens to publish as <c>CNAME</c> records.</param>
public sealed record SesDomainSetupResponse(
    string Domain,
    string VerificationStatus,
    string VerificationToken,
    string DkimVerificationStatus,
    IReadOnlyList<string> DkimTokens);
