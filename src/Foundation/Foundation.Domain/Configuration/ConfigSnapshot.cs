namespace Foundation.Domain.Configuration;

/// <summary>
/// A point-in-time view of the resolved AWS connection configuration.
/// </summary>
/// <param name="AccessKey">The resolved access key (sensitive).</param>
/// <param name="SecretKey">The resolved secret key (sensitive).</param>
/// <param name="ServiceUrl">The resolved service endpoint URL.</param>
/// <param name="Region">The resolved AWS region.</param>
public sealed record ConfigSnapshot(ConfigValue AccessKey, ConfigValue SecretKey, ConfigValue ServiceUrl, ConfigValue Region);
