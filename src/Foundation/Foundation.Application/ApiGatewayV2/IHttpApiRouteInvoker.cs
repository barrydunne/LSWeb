using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.ApiGatewayV2;

/// <summary>
/// Sends a real HTTP request to an Amazon API Gateway v2 route invoke URL so that a route's
/// authorization behaviour can be verified from the UI without external command-line tooling.
/// </summary>
public interface IHttpApiRouteInvoker
{
    /// <summary>
    /// Sends an HTTP request to the supplied route invoke URL.
    /// </summary>
    /// <param name="requestUri">The absolute invoke URL of the route.</param>
    /// <param name="method">The HTTP method to use (for example <c>GET</c> or <c>POST</c>).</param>
    /// <param name="bearerToken">An optional bearer token to send in the <c>Authorization</c> header, or <see langword="null"/> to send an unauthenticated request.</param>
    /// <param name="body">An optional request body, or <see langword="null"/> to send no body.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The invocation result, or an error if the request could not be sent.</returns>
    Task<Result<HttpRouteInvocationResult>> InvokeAsync(
        string requestUri,
        string method,
        string? bearerToken,
        string? body,
        CancellationToken cancellationToken);
}
