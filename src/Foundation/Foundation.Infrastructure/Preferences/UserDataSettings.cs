using System.Diagnostics.CodeAnalysis;

namespace Foundation.Infrastructure.Preferences;

/// <summary>
/// The user-data persistence settings sourced from environment variables. A null
/// <see cref="DataDirectory"/> indicates no host directory was mounted, in which case preferences
/// are kept in memory only.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed record UserDataSettings
{
    public string? DataDirectory { get; init; }
}
