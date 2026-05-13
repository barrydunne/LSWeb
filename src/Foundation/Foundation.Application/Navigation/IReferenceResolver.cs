using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Navigation;

namespace Foundation.Application.Navigation;

/// <summary>
/// Resolves a raw AWS resource reference (an ARN, or a bare identifier qualified by a service) into
/// a navigable <see cref="ResourceReference"/>. Pure logic with no backend calls.
/// </summary>
public interface IReferenceResolver
{
    /// <summary>
    /// Resolves a reference into a navigable target.
    /// </summary>
    /// <param name="reference">An ARN, or the bare identifier of a resource.</param>
    /// <param name="service">The owning service when <paramref name="reference"/> is not an ARN; ignored for ARNs.</param>
    /// <returns>The resolved reference on success; otherwise a failure describing why it could not be resolved.</returns>
    Result<ResourceReference> Resolve(string reference, string? service = null);
}
