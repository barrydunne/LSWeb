namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// The full configuration of an Amazon API Gateway v2 API (HTTP or WebSocket).
/// </summary>
/// <param name="ApiId">The unique identifier of the API.</param>
/// <param name="Name">The human-readable name of the API.</param>
/// <param name="ProtocolType">The protocol of the API (for example <c>HTTP</c> or <c>WEBSOCKET</c>).</param>
/// <param name="ApiEndpoint">The invoke endpoint of the API, or <see langword="null"/> when not reported.</param>
/// <param name="Description">The description of the API, or <see langword="null"/> when not set.</param>
/// <param name="Version">The version identifier of the API, or <see langword="null"/> when not set.</param>
/// <param name="RouteSelectionExpression">The route selection expression of the API, or <see langword="null"/> when not set.</param>
/// <param name="CorsConfiguration">The CORS configuration of the API, or <see langword="null"/> when none is configured.</param>
/// <param name="CreatedDate">The moment the API was created, or <see langword="null"/> when not reported.</param>
public sealed record HttpApiDetail(
    string ApiId,
    string Name,
    string ProtocolType,
    string? ApiEndpoint,
    string? Description,
    string? Version,
    string? RouteSelectionExpression,
    HttpApiCorsConfiguration? CorsConfiguration,
    DateTimeOffset? CreatedDate);
