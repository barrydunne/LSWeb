namespace Foundation.Domain.Catalogue;

/// <summary>
/// Describes a single managed AWS service shown in the service catalogue.
/// </summary>
/// <param name="Key">The stable lowercase identifier for the service (for example <c>s3</c>).</param>
/// <param name="DisplayName">The human-readable service name shown on the dashboard.</param>
/// <param name="Category">The functional category the service belongs to.</param>
/// <param name="IconHint">A hint identifying the icon the UI should render for the service.</param>
/// <param name="Route">The relative SPA route that opens the service's management area.</param>
public sealed record ServiceDescriptor(string Key, string DisplayName, ServiceCategory Category, string IconHint, string Route);
