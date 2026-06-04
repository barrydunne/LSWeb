using System.Diagnostics.CodeAnalysis;
using Foundation.Application.Activity;
using Foundation.Application.ApiGateway;
using Foundation.Application.Capabilities;
using Foundation.Application.CertificateManager;
using Foundation.Application.CloudFormation;
using Foundation.Application.CloudWatchLogs;
using Foundation.Application.Configuration;
using Foundation.Application.Connectivity;
using Foundation.Application.Diagnostics;
using Foundation.Application.DynamoDb;
using Foundation.Application.EventBridge;
using Foundation.Application.Health;
using Foundation.Application.Iam;
using Foundation.Application.Lambda;
using Foundation.Application.Navigation;
using Foundation.Application.Preferences;
using Foundation.Application.Route53;
using Foundation.Application.S3;
using Foundation.Application.Search;
using Foundation.Application.SecretsManager;
using Foundation.Application.Ses;
using Foundation.Application.Sns;
using Foundation.Application.Sqs;
using Foundation.Application.Ssm;
using Foundation.Application.StepFunctions;
using Foundation.Application.Streaming;
using Foundation.Infrastructure.Activity;
using Foundation.Infrastructure.ApiGateway;
using Foundation.Infrastructure.Aws;
using Foundation.Infrastructure.Capabilities;
using Foundation.Infrastructure.CertificateManager;
using Foundation.Infrastructure.CloudFormation;
using Foundation.Infrastructure.CloudWatchLogs;
using Foundation.Infrastructure.Configuration;
using Foundation.Infrastructure.Connectivity;
using Foundation.Infrastructure.Diagnostics;
using Foundation.Infrastructure.DynamoDb;
using Foundation.Infrastructure.Errors;
using Foundation.Infrastructure.EventBridge;
using Foundation.Infrastructure.Health;
using Foundation.Infrastructure.Iam;
using Foundation.Infrastructure.Lambda;
using Foundation.Infrastructure.Navigation;
using Foundation.Infrastructure.Preferences;
using Foundation.Infrastructure.Route53;
using Foundation.Infrastructure.S3;
using Foundation.Infrastructure.Search;
using Foundation.Infrastructure.SecretsManager;
using Foundation.Infrastructure.Ses;
using Foundation.Infrastructure.Sns;
using Foundation.Infrastructure.Sqs;
using Foundation.Infrastructure.Ssm;
using Foundation.Infrastructure.StepFunctions;
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

        var userDataSettings = new UserDataSettings
        {
            DataDirectory = Environment.GetEnvironmentVariable("LSW_USER_DATA_DIR"),
        };

        var redactionSettings = new RedactionSettings(
            string.Equals(Environment.GetEnvironmentVariable("LSW_ALLOW_DIAGNOSTIC_REVEAL"), "true", StringComparison.OrdinalIgnoreCase));

        return services
            .AddSignalR()
            .Services
            .AddSingleton(settings)
            .AddSingleton(userDataSettings)
            .AddSingleton(redactionSettings)
            .AddSingleton<IConfigProvider, ConfigProvider>()
            .AddSingleton<IRedactionService, RedactionService>()
            .AddSingleton<IAwsClientFactory, AwsClientFactory>()
            .AddSingleton<IAwsGateway, AwsGateway>()
            .AddSingleton<IErrorTranslator, ErrorTranslator>()
            .AddSingleton<CapabilityDetector>()
            .AddSingleton<ICapabilityDetector>(_ => _.GetRequiredService<CapabilityDetector>())
            .AddSingleton<ICapabilityProvider>(_ => _.GetRequiredService<CapabilityDetector>())
            .AddSingleton<IConnectivityProbe, ConnectivityProbe>()
            .AddSingleton<ILambdaClient, LambdaClientAdapter>()
            .AddSingleton<IResourceSource, LambdaResourceSource>()
            .AddSingleton<IS3Client, S3ClientAdapter>()
            .AddSingleton<IResourceSource, S3ResourceSource>()
            .AddSingleton<ISqsClient, SqsClientAdapter>()
            .AddSingleton<IResourceSource, SqsResourceSource>()
            .AddSingleton<ICloudWatchLogsClient, CloudWatchLogsClientAdapter>()
            .AddSingleton<IResourceSource, CloudWatchLogsResourceSource>()
            .AddSingleton<IDynamoDbClient, DynamoDbClientAdapter>()
            .AddSingleton<IResourceSource, DynamoDbResourceSource>()
            .AddSingleton<ISecretsManagerClient, SecretsManagerClientAdapter>()
            .AddSingleton<IResourceSource, SecretsManagerResourceSource>()
            .AddSingleton<ISsmClient, SsmClientAdapter>()
            .AddSingleton<IResourceSource, SsmResourceSource>()
            .AddSingleton<ISnsClient, SnsClientAdapter>()
            .AddSingleton<IResourceSource, SnsResourceSource>()
            .AddSingleton<IIamClient, IamClientAdapter>()
            .AddSingleton<IResourceSource, IamResourceSource>()
            .AddSingleton<IStepFunctionsClient, StepFunctionsClientAdapter>()
            .AddSingleton<IResourceSource, StepFunctionsResourceSource>()
            .AddSingleton<ICloudFormationClient, CloudFormationClientAdapter>()
            .AddSingleton<IResourceSource, CloudFormationResourceSource>()
            .AddSingleton<IEventBridgeClient, EventBridgeClientAdapter>()
            .AddSingleton<IResourceSource, EventBridgeResourceSource>()
            .AddSingleton<ICertificateManagerClient, CertificateManagerClientAdapter>()
            .AddSingleton<IResourceSource, CertificateManagerResourceSource>()
            .AddSingleton<IApiGatewayClient, ApiGatewayClientAdapter>()
            .AddSingleton<IResourceSource, ApiGatewayResourceSource>()
            .AddSingleton<IRoute53Client, Route53ClientAdapter>()
            .AddSingleton<IResourceSource, Route53ResourceSource>()
            .AddSingleton<ISesClient, SesClientAdapter>()
            .AddSingleton<IResourceSource, SesResourceSource>()
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
            .AddSingleton<IUserDataStore>(CreateUserDataStore)
            .AddSingleton<ITestEventStore>(CreateTestEventStore)
            .AddHostedService<HealthMonitor>()
            .AddHostedService<SearchIndexer>();
    }

    private static IUserDataStore CreateUserDataStore(IServiceProvider provider)
    {
        var settings = provider.GetRequiredService<UserDataSettings>();
        return string.IsNullOrWhiteSpace(settings.DataDirectory)
            ? ActivatorUtilities.CreateInstance<InMemoryUserDataStore>(provider)
            : ActivatorUtilities.CreateInstance<FileUserDataStore>(provider);
    }

    private static ITestEventStore CreateTestEventStore(IServiceProvider provider)
    {
        var settings = provider.GetRequiredService<UserDataSettings>();
        return string.IsNullOrWhiteSpace(settings.DataDirectory)
            ? ActivatorUtilities.CreateInstance<InMemoryTestEventStore>(provider)
            : ActivatorUtilities.CreateInstance<FileTestEventStore>(provider);
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
