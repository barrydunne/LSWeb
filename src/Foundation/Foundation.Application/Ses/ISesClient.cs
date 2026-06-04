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
}
