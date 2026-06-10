using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateHttpIntegration;

/// <summary>
/// Update an existing Amazon API Gateway v2 integration with the supplied configuration. An
/// integration is the backend target a route forwards requests to.
/// </summary>
/// <param name="ApiId">The identifier of the API the integration belongs to.</param>
/// <param name="IntegrationId">The unique identifier of the integration to update.</param>
/// <param name="IntegrationType">The type of the integration (for example <c>HTTP_PROXY</c>, <c>AWS_PROXY</c>, or <c>MOCK</c>).</param>
/// <param name="IntegrationMethod">The HTTP method used to call the integration target, or <see langword="null"/> when not applicable.</param>
/// <param name="IntegrationUri">The URI of the integration target, or <see langword="null"/> when not applicable.</param>
/// <param name="PayloadFormatVersion">The payload format version of the integration, or <see langword="null"/> to use the backend default.</param>
/// <param name="Description">The description of the integration, or <see langword="null"/> for none.</param>
public record UpdateHttpIntegrationCommand(
    string ApiId,
    string IntegrationId,
    string IntegrationType,
    string? IntegrationMethod,
    string? IntegrationUri,
    string? PayloadFormatVersion,
    string? Description) : ICommand;
