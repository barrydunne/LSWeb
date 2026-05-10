using System.Diagnostics.CodeAnalysis;
using Foundation.Application.Configuration;
using Foundation.Application.Connectivity;
using Foundation.Infrastructure.Aws;
using Foundation.Infrastructure.Configuration;
using Foundation.Infrastructure.Connectivity;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Infrastructure;

/// <summary>
/// Registration of the infrastructure layer services with the dependency injection container.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
public static class DependencyInjection
{
    /// <summary>
    /// Register the infrastructure layer services that provide access to external systems.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddFoundationInfrastructure(this IServiceCollection services)
    {
        var settings = new AwsSettings
        {
            AccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
            SecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
            ServiceUrl = Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL"),
            Region = Environment.GetEnvironmentVariable("AWS_REGION"),
        };

        return services
            .AddSingleton(settings)
            .AddSingleton<IConfigProvider, ConfigProvider>()
            .AddSingleton<IAwsClientFactory, AwsClientFactory>()
            .AddSingleton<IConnectivityProbe, ConnectivityProbe>();
    }
}
