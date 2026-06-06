namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// The cross-origin resource sharing (CORS) configuration of an Amazon API Gateway v2 API.
/// </summary>
/// <param name="AllowCredentials">Whether credentials are allowed in CORS requests, or <see langword="null"/> when not reported.</param>
/// <param name="AllowHeaders">The headers that are allowed in CORS requests.</param>
/// <param name="AllowMethods">The HTTP methods that are allowed in CORS requests.</param>
/// <param name="AllowOrigins">The origins that are allowed to make CORS requests.</param>
/// <param name="ExposeHeaders">The headers that are exposed to the browser in CORS responses.</param>
/// <param name="MaxAge">The number of seconds a browser may cache the CORS preflight response, or <see langword="null"/> when not reported.</param>
public sealed record HttpApiCorsConfiguration(
    bool? AllowCredentials,
    IReadOnlyList<string> AllowHeaders,
    IReadOnlyList<string> AllowMethods,
    IReadOnlyList<string> AllowOrigins,
    IReadOnlyList<string> ExposeHeaders,
    int? MaxAge);
