namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// A concise view of an Amazon API Gateway v2 API (HTTP or WebSocket) as it appears in a list.
/// </summary>
/// <param name="ApiId">The unique identifier of the API.</param>
/// <param name="Name">The human-readable name of the API.</param>
/// <param name="ProtocolType">The protocol of the API (for example <c>HTTP</c> or <c>WEBSOCKET</c>).</param>
/// <param name="ApiEndpoint">The invoke endpoint of the API, or <see langword="null"/> when not reported.</param>
/// <param name="CreatedDate">The moment the API was created, or <see langword="null"/> when not reported.</param>
public sealed record HttpApiSummary(
    string ApiId,
    string Name,
    string ProtocolType,
    string? ApiEndpoint,
    DateTimeOffset? CreatedDate);
