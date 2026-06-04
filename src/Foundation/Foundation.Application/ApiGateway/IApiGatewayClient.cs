using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.ApiGateway;

/// <summary>
/// Abstracts the API Gateway operations the application needs so the handlers stay free of any
/// direct AWS SDK dependency. The implementation flows every call through the resilient AWS gateway
/// and translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface IApiGatewayClient
{
    /// <summary>
    /// List the REST APIs available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The REST APIs, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<RestApi>>> ListRestApisAsync(
        CancellationToken cancellationToken);
}
