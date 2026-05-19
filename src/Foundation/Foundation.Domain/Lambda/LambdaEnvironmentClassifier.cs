namespace Foundation.Domain.Lambda;

/// <summary>
/// Classifies Lambda environment variable names as sensitive based on well-known naming substrings.
/// </summary>
public static class LambdaEnvironmentClassifier
{
    private static readonly string[] _sensitiveMarkers =
    [
        "secret",
        "password",
        "passwd",
        "token",
        "key",
        "credential",
        "private",
    ];

    /// <summary>
    /// Determines whether an environment variable name indicates a sensitive value.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    /// <returns><see langword="true"/> when the name matches a known sensitive marker; otherwise <see langword="false"/>.</returns>
    public static bool IsSensitive(string name) =>
        _sensitiveMarkers.Any(marker => name.Contains(marker, StringComparison.OrdinalIgnoreCase));
}
