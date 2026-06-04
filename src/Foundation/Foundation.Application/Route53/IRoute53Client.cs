using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Route53;

namespace Foundation.Application.Route53;

/// <summary>
/// Abstracts the Route 53 operations the application needs so the handlers stay free of any direct
/// AWS SDK dependency. The implementation flows every call through the resilient AWS gateway and
/// translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface IRoute53Client
{
    /// <summary>
    /// List the hosted zones available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The hosted zones, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<HostedZone>>> ListHostedZonesAsync(
        CancellationToken cancellationToken);
}
