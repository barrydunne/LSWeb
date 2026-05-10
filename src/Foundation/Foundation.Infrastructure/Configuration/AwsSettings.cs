namespace Foundation.Infrastructure.Configuration;

/// <summary>
/// The raw AWS connection settings sourced from environment variables.
/// A null property indicates the corresponding variable was not supplied.
/// </summary>
internal sealed record AwsSettings
{
    public string? AccessKey { get; init; }

    public string? SecretKey { get; init; }

    public string? ServiceUrl { get; init; }

    public string? Region { get; init; }
}
