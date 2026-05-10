using System.Collections.Concurrent;
using Foundation.Application.Capabilities;
using Foundation.Domain.Capabilities;
using Foundation.Domain.Catalogue;
using Foundation.Domain.Errors;

namespace Foundation.Infrastructure.Capabilities;

/// <summary>
/// Builds the transient <see cref="CapabilityMap"/> from observed operation outcomes. Every
/// catalogue service starts as <see cref="CapabilityStatus.Unknown"/> and is promoted to
/// supported or unsupported as the backend is exercised.
/// </summary>
internal sealed class CapabilityDetector : ICapabilityDetector, ICapabilityProvider
{
    private const string UnsupportedDetail = "Not supported by the current backend.";

    private readonly ConcurrentDictionary<string, CapabilityStatus> _statuses;

    public CapabilityDetector()
    {
        _statuses = new ConcurrentDictionary<string, CapabilityStatus>(StringComparer.Ordinal);
        foreach (var service in ServiceCatalogue.Services)
        {
            _statuses[service.Key] = CapabilityStatus.Unknown;
        }
    }

    /// <inheritdoc />
    public void RecordSuccess(string serviceKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceKey);
        _statuses[serviceKey] = CapabilityStatus.Supported;
    }

    /// <inheritdoc />
    public void RecordError(string serviceKey, ErrorModel error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceKey);
        ArgumentNullException.ThrowIfNull(error);
        _statuses[serviceKey] = error.Category == ErrorCategory.Unsupported
            ? CapabilityStatus.Unsupported
            : CapabilityStatus.Supported;
    }

    /// <inheritdoc />
    public CapabilityMap GetCapabilities()
    {
        var entries = _statuses
            .Select(pair => new CapabilityEntry(pair.Key, pair.Value, DetailFor(pair.Value)))
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .ToList();

        return new CapabilityMap(entries);
    }

    private static string? DetailFor(CapabilityStatus status)
        => status == CapabilityStatus.Unsupported ? UnsupportedDetail : null;
}
