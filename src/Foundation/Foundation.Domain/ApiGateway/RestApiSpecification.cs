namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The desired configuration of an API Gateway REST API when creating or updating one.
/// </summary>
/// <param name="Id">The identifier of the REST API to update, or <see langword="null"/> when creating a new REST API.</param>
/// <param name="Name">The name of the REST API.</param>
/// <param name="Description">The description of the REST API, or <see langword="null"/> for none.</param>
/// <param name="Version">The version identifier of the REST API, or <see langword="null"/> for none.</param>
/// <param name="ApiKeySource">The source of the API key for metering requests, or <see langword="null"/> to use the backend default.</param>
/// <param name="EndpointConfigurationTypes">The endpoint types of the REST API (for example <c>EDGE</c>, <c>REGIONAL</c> or <c>PRIVATE</c>).</param>
public sealed record RestApiSpecification(
    string? Id,
    string Name,
    string? Description,
    string? Version,
    string? ApiKeySource,
    IReadOnlyList<string> EndpointConfigurationTypes);
