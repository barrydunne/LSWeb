namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The full configuration of an API Gateway REST API.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the REST API.</param>
/// <param name="Name">The name of the REST API.</param>
/// <param name="Description">An optional human-readable description, or <see langword="null"/> when none is set.</param>
/// <param name="Version">The version identifier of the REST API, or <see langword="null"/> when not set.</param>
/// <param name="ApiKeySource">The source of the API key for metering requests, or <see langword="null"/> when not reported.</param>
/// <param name="EndpointConfigurationTypes">The endpoint types of the REST API (for example <c>EDGE</c>, <c>REGIONAL</c> or <c>PRIVATE</c>).</param>
/// <param name="BinaryMediaTypes">The list of binary media types supported by the REST API.</param>
/// <param name="CreatedDate">The UTC creation timestamp, or <see langword="null"/> when not reported.</param>
public sealed record RestApiDetail(
    string Id,
    string Name,
    string? Description,
    string? Version,
    string? ApiKeySource,
    IReadOnlyList<string> EndpointConfigurationTypes,
    IReadOnlyList<string> BinaryMediaTypes,
    DateTimeOffset? CreatedDate);
