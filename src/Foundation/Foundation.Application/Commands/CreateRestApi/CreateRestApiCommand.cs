using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateRestApi;

/// <summary>
/// Create an API Gateway REST API from the supplied configuration.
/// </summary>
/// <param name="Name">The name of the REST API to create.</param>
/// <param name="Description">The description of the REST API, or <see langword="null"/> for none.</param>
/// <param name="Version">The version identifier of the REST API, or <see langword="null"/> for none.</param>
/// <param name="ApiKeySource">The source of the API key for metering requests, or <see langword="null"/> to use the backend default.</param>
/// <param name="EndpointConfigurationTypes">The endpoint types of the REST API (for example <c>EDGE</c>, <c>REGIONAL</c> or <c>PRIVATE</c>).</param>
public record CreateRestApiCommand(
    string Name,
    string? Description,
    string? Version,
    string? ApiKeySource,
    IReadOnlyList<string> EndpointConfigurationTypes) : ICommand<string>;
