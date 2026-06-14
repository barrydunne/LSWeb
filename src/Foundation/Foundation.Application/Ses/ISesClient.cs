using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Ses;

namespace Foundation.Application.Ses;

/// <summary>
/// Abstracts the SES operations the application needs so the handlers stay free of any direct AWS
/// SDK dependency. The implementation flows every call through the resilient AWS gateway and
/// translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface ISesClient
{
    /// <summary>
    /// List the SES identities (email addresses and domains) available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identities, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<SesIdentity>>> ListIdentitiesAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Get the verification detail for a single SES identity.
    /// </summary>
    /// <param name="identity">The email address or domain name to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identity detail, or an error when the backend cannot be reached.</returns>
    Task<Result<SesIdentityDetail>> GetIdentityAsync(
        string identity,
        CancellationToken cancellationToken);

    /// <summary>
    /// Start the verification of an email address identity. On real AWS this sends a verification
    /// email to the address; on LocalStack the identity is registered immediately.
    /// </summary>
    /// <param name="emailAddress">The email address to verify.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the backend cannot be reached.</returns>
    Task<Result> VerifyEmailIdentityAsync(
        string emailAddress,
        CancellationToken cancellationToken);

    /// <summary>
    /// Delete an SES identity (an email address or domain) from the backend.
    /// </summary>
    /// <param name="identity">The email address or domain name to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the backend cannot be reached.</returns>
    Task<Result> DeleteIdentityAsync(
        string identity,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get the domain verification and DKIM setup state for a domain identity.
    /// </summary>
    /// <param name="domain">The domain name to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The domain setup state, or an error when the backend cannot be reached.</returns>
    Task<Result<SesDomainSetup>> GetDomainSetupAsync(
        string domain,
        CancellationToken cancellationToken);

    /// <summary>
    /// Initiate the verification of a domain identity. The backend returns a verification token
    /// that must be published as a DNS <c>TXT</c> record.
    /// </summary>
    /// <param name="domain">The domain name to verify.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the backend cannot be reached.</returns>
    Task<Result> VerifyDomainIdentityAsync(
        string domain,
        CancellationToken cancellationToken);

    /// <summary>
    /// Enable DKIM signing for a domain identity. The backend returns the DKIM tokens that must be
    /// published as DNS <c>CNAME</c> records.
    /// </summary>
    /// <param name="domain">The domain name to enable DKIM for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the backend cannot be reached.</returns>
    Task<Result> EnableDomainDkimAsync(
        string domain,
        CancellationToken cancellationToken);
}
