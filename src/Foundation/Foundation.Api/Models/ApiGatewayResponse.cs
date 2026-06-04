namespace Foundation.Api.Models;

/// <summary>
/// The API Gateway REST APIs available on the backend.
/// </summary>
/// <param name="RestApis">The REST API summaries, ordered as returned by the backend.</param>
public sealed record RestApiListResponse(
    IReadOnlyList<RestApiSummaryResponse> RestApis);

/// <summary>
/// A concise view of an API Gateway REST API as it appears in a list.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the REST API.</param>
/// <param name="Name">The name of the REST API.</param>
/// <param name="Description">An optional human-readable description, or <c>null</c> when none is set.</param>
/// <param name="CreatedDate">The UTC creation timestamp, or <c>null</c> when not reported.</param>
public sealed record RestApiSummaryResponse(
    string Id,
    string Name,
    string? Description,
    DateTimeOffset? CreatedDate);
