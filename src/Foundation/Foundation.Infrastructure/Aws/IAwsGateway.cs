using Amazon.Runtime;
using AspNet.KickStarter.FunctionalResult;

namespace Foundation.Infrastructure.Aws;

/// <summary>
/// A resilient entry point for invoking AWS SDK operations. Wraps the client factory
/// with a retry and circuit-breaker pipeline and converts failures into a
/// <see cref="Result{T}"/> rather than throwing across layers.
/// </summary>
internal interface IAwsGateway
{
    /// <summary>
    /// Executes an operation against a resiliently-created AWS client.
    /// </summary>
    /// <typeparam name="TClient">The AWS SDK client type to resolve.</typeparam>
    /// <typeparam name="TResult">The operation result type.</typeparam>
    /// <param name="serviceKey">The catalogue service key the operation belongs to, used to record capability.</param>
    /// <param name="operation">The operation to invoke against the resolved client.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The operation result on success; otherwise a failure describing the error.</returns>
    Task<Result<TResult>> ExecuteAsync<TClient, TResult>(
        string serviceKey,
        Func<TClient, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
        where TClient : AmazonServiceClient;
}
