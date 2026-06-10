using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Commands.TestHttpRoute;

/// <summary>
/// Invoke an Amazon API Gateway v2 route to verify its authorization behaviour.
/// </summary>
/// <param name="ApiId">The identifier of the API the route belongs to.</param>
/// <param name="Stage">The stage to invoke (for example <c>$default</c> or a named stage).</param>
/// <param name="Method">The HTTP method to use (for example <c>GET</c> or <c>POST</c>).</param>
/// <param name="Path">The request path to invoke (for example <c>/items</c>).</param>
/// <param name="Token">An optional bearer token to send, or <see langword="null"/> to send an unauthenticated request.</param>
/// <param name="Body">An optional request body, or <see langword="null"/> to send no body.</param>
public record TestHttpRouteCommand(
    string ApiId,
    string Stage,
    string Method,
    string Path,
    string? Token,
    string? Body) : ICommand<HttpRouteInvocationResult>;
