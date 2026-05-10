using Amazon.Runtime;

namespace Foundation.Infrastructure.Aws;

/// <summary>
/// Creates AWS SDK service clients configured for the resolved connection settings.
/// </summary>
internal interface IAwsClientFactory
{
    /// <summary>
    /// Get a cached AWS SDK client for the requested service type.
    /// </summary>
    /// <typeparam name="T">The concrete AWS SDK client type.</typeparam>
    /// <returns>A shared, thread-safe client instance.</returns>
    T CreateClient<T>()
        where T : AmazonServiceClient;
}
