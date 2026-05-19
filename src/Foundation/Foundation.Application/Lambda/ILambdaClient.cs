using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Lambda;

/// <summary>
/// Reads Lambda function metadata from the configured AWS backend. Implementations route through
/// the resilient AWS gateway and never throw across layers, reporting failures as a
/// <see cref="Result{T}"/>.
/// </summary>
public interface ILambdaClient
{
    /// <summary>
    /// List the Lambda functions visible to the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The function summaries on success; otherwise a failure describing the error.</returns>
    Task<Result<IReadOnlyList<LambdaFunctionSummary>>> ListFunctionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get the full configuration of a single Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The function detail on success; otherwise a failure describing the error.</returns>
    Task<Result<LambdaFunctionDetail>> GetFunctionAsync(string functionName, CancellationToken cancellationToken);

    /// <summary>
    /// Get the raw, unmasked environment variables configured for a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The environment variables on success; otherwise a failure describing the error.</returns>
    Task<Result<IReadOnlyDictionary<string, string>>> GetEnvironmentAsync(string functionName, CancellationToken cancellationToken);

    /// <summary>
    /// Replace the environment variables configured for a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function to update.</param>
    /// <param name="variables">The full set of environment variables to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> UpdateEnvironmentAsync(string functionName, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken);

    /// <summary>
    /// Invoke a Lambda function synchronously (request/response) with the supplied payload.
    /// </summary>
    /// <param name="functionName">The name of the function to invoke.</param>
    /// <param name="payload">The JSON payload to send to the function.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The invocation result on success; otherwise a failure describing the error.</returns>
    Task<Result<LambdaInvocationResult>> InvokeAsync(string functionName, string payload, CancellationToken cancellationToken);

    /// <summary>
    /// Create a new Lambda function from the supplied specification.
    /// </summary>
    /// <param name="spec">The function definition, including the deployment package.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> CreateFunctionAsync(LambdaFunctionCreateSpec spec, CancellationToken cancellationToken);

    /// <summary>
    /// Update the configuration of an existing Lambda function.
    /// </summary>
    /// <param name="spec">The configuration values to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> UpdateConfigurationAsync(LambdaConfigurationUpdateSpec spec, CancellationToken cancellationToken);

    /// <summary>
    /// Replace the deployment package (code) of an existing Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function to update.</param>
    /// <param name="zipFileBase64">The deployment package as a base64-encoded ZIP archive.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> UpdateCodeAsync(string functionName, string zipFileBase64, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> DeleteFunctionAsync(string functionName, CancellationToken cancellationToken);

    /// <summary>
    /// List the event source mappings configured for a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function whose mappings to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The event source mappings on success; otherwise a failure describing the error.</returns>
    Task<Result<IReadOnlyList<LambdaEventSourceMapping>>> ListEventSourceMappingsAsync(string functionName, CancellationToken cancellationToken);

    /// <summary>
    /// Enable or disable an existing Lambda event source mapping.
    /// </summary>
    /// <param name="uuid">The unique identifier of the mapping to update.</param>
    /// <param name="enabled">Whether the mapping should be enabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> SetEventSourceMappingStateAsync(string uuid, bool enabled, CancellationToken cancellationToken);

    /// <summary>
    /// Read the most recent CloudWatch log events emitted by a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function whose log events to read.</param>
    /// <param name="limit">The maximum number of log events to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The log events on success; otherwise a failure describing the error.</returns>
    Task<Result<IReadOnlyList<LambdaLogEvent>>> GetRecentLogEventsAsync(string functionName, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// List the layer versions attached to a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the function whose layers to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The attached layers on success; otherwise a failure describing the error.</returns>
    Task<Result<IReadOnlyList<LambdaLayer>>> ListLayersAsync(string functionName, CancellationToken cancellationToken);
}
