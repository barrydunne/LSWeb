namespace Foundation.Domain.Ses;

/// <summary>
/// A concise view of an SES identity (an email address or domain) as it appears in a list.
/// </summary>
/// <param name="Identity">The email address or domain name of the identity.</param>
/// <param name="IdentityType">The identity type, either <c>EmailAddress</c> or <c>Domain</c>.</param>
/// <param name="VerificationStatus">The verification status, such as <c>Success</c> or <c>Pending</c>.</param>
public sealed record SesIdentity(
    string Identity,
    string IdentityType,
    string VerificationStatus);
