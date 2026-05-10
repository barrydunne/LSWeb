using System.Collections.Concurrent;
using Amazon.Runtime;
using Foundation.Application.Configuration;

namespace Foundation.Infrastructure.Aws;

/// <summary>
/// Creates and caches AWS SDK service clients using only the standard SDK, so the
/// same code targets LocalStack or real AWS based purely on the resolved configuration.
/// </summary>
internal sealed class AwsClientFactory : IAwsClientFactory, IDisposable
{
    private readonly IConfigProvider _configProvider;
    private readonly ConcurrentDictionary<Type, AmazonServiceClient> _clients = new();

    public AwsClientFactory(IConfigProvider configProvider)
        => _configProvider = configProvider;

    public T CreateClient<T>()
        where T : AmazonServiceClient
        => (T)_clients.GetOrAdd(typeof(T), _ => Build<T>());

    public void Dispose()
    {
        foreach (var client in _clients.Values)
            client.Dispose();

        _clients.Clear();
    }

    private T Build<T>()
        where T : AmazonServiceClient
    {
        var snapshot = _configProvider.GetSnapshot();
        var credentials = new BasicAWSCredentials(snapshot.AccessKey.Value, snapshot.SecretKey.Value);
        var config = CreateConfig(typeof(T), snapshot.ServiceUrl.Value, snapshot.Region.Value);
        return (T)Activator.CreateInstance(typeof(T), credentials, config)!;
    }

    private static ClientConfig CreateConfig(Type clientType, string serviceUrl, string region)
    {
        var configType = clientType.GetConstructors()
            .Select(_ => _.GetParameters())
            .Where(_ => _.Length == 2
                && typeof(AWSCredentials).IsAssignableFrom(_[0].ParameterType)
                && typeof(ClientConfig).IsAssignableFrom(_[1].ParameterType))
            .Select(_ => _[1].ParameterType)
            .First();

        var config = (ClientConfig)Activator.CreateInstance(configType)!;
        config.ServiceURL = serviceUrl;
        config.AuthenticationRegion = region;
        return config;
    }
}
