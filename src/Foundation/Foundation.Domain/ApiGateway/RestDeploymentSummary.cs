namespace Foundation.Domain.ApiGateway;

/// <summary>
/// A concise view of an API Gateway REST API deployment as it appears in a list.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the deployment.</param>
/// <param name="Description">An optional human-readable description, or <see langword="null"/> when none is set.</param>
/// <param name="CreatedDate">The UTC creation timestamp, or <see langword="null"/> when not reported.</param>
public sealed record RestDeploymentSummary(
    string Id,
    string? Description,
    DateTimeOffset? CreatedDate);
