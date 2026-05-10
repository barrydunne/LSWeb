namespace Foundation.Domain.Health;

/// <summary>
/// The resolved availability of a single backend service.
/// </summary>
/// <param name="Key">The catalogue service key, for example <c>s3</c>.</param>
/// <param name="Availability">Whether the service is currently usable.</param>
public sealed record ServiceHealth(string Key, ServiceAvailability Availability);
