using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Commands.UpdateHttpApi;

/// <summary>
/// Update the configuration of an existing Amazon API Gateway v2 API.
/// </summary>
/// <param name="ApiId">The unique identifier of the API to update.</param>
/// <param name="Name">The name of the API.</param>
/// <param name="ProtocolType">The protocol of the API (for example <c>HTTP</c> or <c>WEBSOCKET</c>).</param>
/// <param name="Description">The description of the API, or <see langword="null"/> for none.</param>
/// <param name="Version">The version identifier of the API, or <see langword="null"/> for none.</param>
/// <param name="RouteSelectionExpression">The route selection expression of the API, or <see langword="null"/> to use the backend default.</param>
/// <param name="CorsConfiguration">The CORS configuration to apply, or <see langword="null"/> to leave the CORS configuration unchanged.</param>
public record UpdateHttpApiCommand(
    string ApiId,
    string Name,
    string ProtocolType,
    string? Description,
    string? Version,
    string? RouteSelectionExpression,
    HttpApiCorsConfiguration? CorsConfiguration = null) : ICommand;
