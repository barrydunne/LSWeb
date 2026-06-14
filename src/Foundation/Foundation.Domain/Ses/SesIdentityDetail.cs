namespace Foundation.Domain.Ses;

/// <summary>
/// A detailed view of a single SES identity (an email address or domain) and where it sits in the
/// verification lifecycle.
/// </summary>
/// <param name="Identity">The email address or domain name of the identity.</param>
/// <param name="IdentityType">The identity type, either <c>EmailAddress</c> or <c>Domain</c>.</param>
/// <param name="VerificationStatus">
/// The verification status, such as <c>Success</c>, <c>Pending</c>, <c>Failed</c>, or
/// <c>NotStarted</c> when the identity has no verification on record.
/// </param>
public sealed record SesIdentityDetail(
    string Identity,
    string IdentityType,
    string VerificationStatus);
