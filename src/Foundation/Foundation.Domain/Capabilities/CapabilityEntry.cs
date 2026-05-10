namespace Foundation.Domain.Capabilities;

/// <summary>
/// The detected capability of a single service.
/// </summary>
/// <param name="Key">The catalogue service key, for example <c>s3</c>.</param>
/// <param name="Status">Whether the backend supports the service.</param>
/// <param name="Detail">An optional human-readable note, for example why a service is unsupported.</param>
public sealed record CapabilityEntry(string Key, CapabilityStatus Status, string? Detail);
