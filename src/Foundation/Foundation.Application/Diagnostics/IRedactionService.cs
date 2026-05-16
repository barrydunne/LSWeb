using Foundation.Domain.Configuration;

namespace Foundation.Application.Diagnostics;

/// <summary>
/// Redacts sensitive configuration values, optionally permitting an explicit, guarded reveal.
/// </summary>
public interface IRedactionService
{
    /// <summary>
    /// Gets a value indicating whether the host permits sensitive values to be revealed.
    /// </summary>
    bool CanReveal { get; }

    /// <summary>
    /// Resolve the display form of a configuration value, masking sensitive values unless a
    /// reveal is both requested and permitted by the host.
    /// </summary>
    /// <param name="value">The configuration value to resolve.</param>
    /// <param name="reveal">Whether the caller has explicitly requested the unmasked value.</param>
    /// <returns>The unmasked value when reveal is requested and permitted; otherwise the masked form.</returns>
    string Resolve(ConfigValue value, bool reveal);
}
