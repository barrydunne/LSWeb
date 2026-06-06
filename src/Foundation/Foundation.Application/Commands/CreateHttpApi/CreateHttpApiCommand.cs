using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateHttpApi;

/// <summary>
/// Create an Amazon API Gateway v2 API from the supplied configuration.
/// </summary>
/// <param name="Name">The name of the API to create.</param>
/// <param name="ProtocolType">The protocol of the API (for example <c>HTTP</c> or <c>WEBSOCKET</c>).</param>
/// <param name="Description">The description of the API, or <see langword="null"/> for none.</param>
/// <param name="Version">The version identifier of the API, or <see langword="null"/> for none.</param>
/// <param name="RouteSelectionExpression">The route selection expression of the API, or <see langword="null"/> to use the backend default.</param>
public record CreateHttpApiCommand(
    string Name,
    string ProtocolType,
    string? Description,
    string? Version,
    string? RouteSelectionExpression) : ICommand<string>;
