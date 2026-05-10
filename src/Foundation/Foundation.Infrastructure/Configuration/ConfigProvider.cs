using Foundation.Application.Configuration;
using Foundation.Domain.Configuration;

namespace Foundation.Infrastructure.Configuration;

/// <summary>
/// Resolves the AWS connection configuration from environment-sourced settings,
/// applying built-in defaults and recording the origin of each value.
/// </summary>
internal sealed class ConfigProvider : IConfigProvider
{
    private const string DefaultAccessKey = "test";
    private const string DefaultSecretKey = "test";
    private const string DefaultServiceUrl = "http://host.docker.internal:4566";
    private const string DefaultRegion = "eu-west-1";

    private readonly ConfigSnapshot _snapshot;

    public ConfigProvider(AwsSettings settings)
    {
        _snapshot = new ConfigSnapshot(
            Resolve(nameof(AwsSettings.AccessKey), settings.AccessKey, DefaultAccessKey, isSensitive: true),
            Resolve(nameof(AwsSettings.SecretKey), settings.SecretKey, DefaultSecretKey, isSensitive: true),
            Resolve(nameof(AwsSettings.ServiceUrl), settings.ServiceUrl, DefaultServiceUrl, isSensitive: false),
            Resolve(nameof(AwsSettings.Region), settings.Region, DefaultRegion, isSensitive: false));
    }

    public ConfigSnapshot GetSnapshot() => _snapshot;

    private static ConfigValue Resolve(string name, string? value, string defaultValue, bool isSensitive)
        => string.IsNullOrWhiteSpace(value)
            ? new ConfigValue(name, defaultValue, ConfigSource.Default, isSensitive)
            : new ConfigValue(name, value, ConfigSource.EnvironmentVariable, isSensitive);
}
