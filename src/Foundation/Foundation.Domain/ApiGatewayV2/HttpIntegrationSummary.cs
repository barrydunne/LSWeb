namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// A concise view of an Amazon API Gateway v2 integration as it appears in a list. An integration
/// is the backend target a route forwards requests to.
/// </summary>
/// <param name="IntegrationId">The unique identifier of the integration.</param>
/// <param name="IntegrationType">The type of the integration (for example <c>HTTP_PROXY</c>, <c>AWS_PROXY</c>, or <c>MOCK</c>).</param>
/// <param name="IntegrationMethod">The HTTP method used to call the integration target, or <see langword="null"/> when not applicable.</param>
/// <param name="IntegrationUri">The URI of the integration target, or <see langword="null"/> when not applicable.</param>
/// <param name="PayloadFormatVersion">The payload format version of the integration, or <see langword="null"/> when not reported.</param>
/// <param name="Description">The description of the integration, or <see langword="null"/> when not set.</param>
public sealed record HttpIntegrationSummary(
    string IntegrationId,
    string IntegrationType,
    string? IntegrationMethod,
    string? IntegrationUri,
    string? PayloadFormatVersion,
    string? Description);
