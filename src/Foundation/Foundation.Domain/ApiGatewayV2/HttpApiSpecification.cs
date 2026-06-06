namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// The desired configuration of an Amazon API Gateway v2 API when creating or updating one.
/// </summary>
/// <param name="ApiId">The identifier of the API to update, or <see langword="null"/> when creating a new API.</param>
/// <param name="Name">The name of the API.</param>
/// <param name="ProtocolType">The protocol of the API (for example <c>HTTP</c> or <c>WEBSOCKET</c>).</param>
/// <param name="Description">The description of the API, or <see langword="null"/> for none.</param>
/// <param name="Version">The version identifier of the API, or <see langword="null"/> for none.</param>
/// <param name="RouteSelectionExpression">The route selection expression of the API, or <see langword="null"/> to use the backend default.</param>
public sealed record HttpApiSpecification(
    string? ApiId,
    string Name,
    string ProtocolType,
    string? Description,
    string? Version,
    string? RouteSelectionExpression);
