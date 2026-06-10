using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteHttpIntegration;

/// <summary>
/// Delete an Amazon API Gateway v2 integration. This action cannot be undone.
/// </summary>
/// <param name="ApiId">The identifier of the API the integration belongs to.</param>
/// <param name="IntegrationId">The unique identifier of the integration to delete.</param>
public record DeleteHttpIntegrationCommand(string ApiId, string IntegrationId) : ICommand;
