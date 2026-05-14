using System.Diagnostics.CodeAnalysis;
using Foundation.Application.Activity;
using Foundation.Application.Capabilities;
using Foundation.Application.Configuration;
using Foundation.Application.Connectivity;
using Foundation.Application.Health;
using Foundation.Application.Navigation;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Infrastructure.Activity;
using Foundation.Infrastructure.Aws;
using Foundation.Infrastructure.Capabilities;
using Foundation.Infrastructure.Configuration;
using Foundation.Infrastructure.Connectivity;
using Foundation.Infrastructure.Errors;
using Foundation.Infrastructure.Health;
using Foundation.Infrastructure.Navigation;
using Foundation.Infrastructure.Search;
using Foundation.Infrastructure.Streaming;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
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
            .AddSignalR()
            .Services
            .AddSingleton(settings)
            .AddSingleton<IConfigProvider, ConfigProvider>()
            .AddSingleton<IAwsClientFactory, AwsClientFactory>()
            .AddSingleton<IAwsGateway, AwsGateway>()
            .AddSingleton<IErrorTranslator, ErrorTranslator>()
            .AddSingleton<CapabilityDetector>()
            .AddSingleton<ICapabilityDetector>(_ => _.GetRequiredService<CapabilityDetector>())
            .AddSingleton<ICapabilityProvider>(_ => _.GetRequiredService<CapabilityDetector>())
            .AddSingleton<IConnectivityProbe, ConnectivityProbe>()
            .AddSingleton<IReferenceResolver, ReferenceResolver>()
            .AddSingleton<IBackendHealthProbe, BackendHealthProbe>()
            .AddSingleton<HealthStatusStore>()
            .AddSingleton<IHealthStatusProvider>(_ => _.GetRequiredService<HealthStatusStore>())
            .AddSingleton<IndexStore>()
            .AddSingleton<ISearchIndexProvider>(_ => _.GetRequiredService<IndexStore>())
            .AddSingleton<SearchIndexCoordinator>()
            .AddSingleton<ISearchIndexSignals>(_ => _.GetRequiredService<SearchIndexCoordinator>())
            .AddSingleton<ISearchRefreshTrigger>(_ => _.GetRequiredService<SearchIndexCoordinator>())
            .AddSingleton(TimeProvider.System)
            .AddSingleton<SearchIndexBuilder>()
            .AddSingleton<StreamSessionManager>()
            .AddSingleton<INotificationPublisher, NotificationPublisher>()
            .AddSingleton<IActivityLog, ActivityLog>()
            .AddHostedService<HealthMonitor>()
            .AddHostedService<SearchIndexer>();
    }

    /// <summary>
    /// Map the real-time streaming endpoints that connected clients subscribe to.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to register the hub with.</param>
    /// <returns>The same endpoint route builder so that multiple calls can be chained.</returns>
    public static IEndpointRouteBuilder MapFoundationStreaming(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<StreamHub>("/hub/stream");
        return endpoints;
    }
}
