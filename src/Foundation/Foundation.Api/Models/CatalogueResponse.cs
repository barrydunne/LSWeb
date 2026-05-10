namespace Foundation.Api.Models;

/// <summary>
/// The catalogue of managed AWS services.
/// </summary>
/// <param name="Services">The managed services in display order.</param>
public sealed record CatalogueResponse(IReadOnlyList<CatalogueServiceResponse> Services);

/// <summary>
/// A single managed AWS service in the catalogue.
/// </summary>
/// <param name="Key">The stable lowercase identifier for the service.</param>
/// <param name="DisplayName">The human-readable service name.</param>
/// <param name="Category">The functional category the service belongs to.</param>
/// <param name="IconHint">A hint identifying the icon the UI should render.</param>
/// <param name="Route">The relative SPA route that opens the service's management area.</param>
/// <param name="Supported">Whether the running backend supports this service. Unsupported services are non-actionable.</param>
/// <param name="SupportDetail">A human-readable explanation shown when the service is not supported; otherwise <see langword="null"/>.</param>
public sealed record CatalogueServiceResponse(
    string Key,
    string DisplayName,
    string Category,
    string IconHint,
    string Route,
    bool Supported,
    string? SupportDetail);
