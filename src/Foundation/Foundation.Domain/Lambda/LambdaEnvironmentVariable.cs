namespace Foundation.Domain.Lambda;

/// <summary>
/// A single Lambda environment variable together with whether its value is sensitive and therefore
/// must be masked when displayed.
/// </summary>
/// <param name="Name">The environment variable name.</param>
/// <param name="Value">The value to display; sensitive values are masked by the presentation layer.</param>
/// <param name="IsSensitive">Whether the variable is considered sensitive.</param>
public sealed record LambdaEnvironmentVariable(string Name, string Value, bool IsSensitive);
