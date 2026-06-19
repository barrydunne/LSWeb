using Amazon.Runtime;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Resilience;
using Foundation.Infrastructure.Capabilities;
using Foundation.Infrastructure.Errors;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Foundation.Infrastructure.Aws;

/// <summary>
/// Wraps the AWS client factory with a Polly resilience pipeline (an outer circuit-breaker
/// guarding an inner retry with exponential backoff and jitter) and surfaces failures as a
/// <see cref="Result{T}"/> so callers never have to catch exceptions across layers. Each
/// outcome is fed to the capability detector and failures are translated into a friendly
/// <see cref="Result{T}"/> error.
/// </summary>
internal sealed partial class AwsGateway : IAwsGateway, ICircuitBreakerReset
{
    private const int MaxAttempts = 3;

    private readonly IAwsClientFactory _clientFactory;
    private readonly IErrorTranslator _errorTranslator;
    private readonly ICapabilityDetector _capabilityDetector;
    private readonly ICircuitBreakerMonitor _circuitBreakerMonitor;
    private readonly CircuitBreakerManualControl _manualControl = new();
    private readonly ILogger _logger;
    private readonly ResiliencePipeline _pipeline;

    public AwsGateway(
        IAwsClientFactory clientFactory,
        IErrorTranslator errorTranslator,
        ICapabilityDetector capabilityDetector,
        ICircuitBreakerMonitor circuitBreakerMonitor,
        ILogger<AwsGateway> logger)
    {
        _clientFactory = clientFactory;
        _errorTranslator = errorTranslator;
        _capabilityDetector = capabilityDetector;
        _circuitBreakerMonitor = circuitBreakerMonitor;
        _logger = logger;
        _pipeline = BuildPipeline(_manualControl);
    }

    public async Task<Result<TResult>> ExecuteAsync<TClient, TResult>(
        string serviceKey,
        Func<TClient, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
        where TClient : AmazonServiceClient
    {
        try
        {
            var client = _clientFactory.CreateClient<TClient>();
            var value = await _pipeline.ExecuteAsync(
                async token => await operation(client, token),
                cancellationToken);
            _capabilityDetector.RecordSuccess(serviceKey);
            _circuitBreakerMonitor.RecordRecovered(serviceKey);
            return value;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            if (exception is BrokenCircuitException)
                _circuitBreakerMonitor.RecordSuspended(serviceKey);

            var error = _errorTranslator.Translate(exception);
            _capabilityDetector.RecordError(serviceKey, error);
            LogOperationFailed(serviceKey, error.Code, error.Message);
            return new Error(error.Message);
        }
    }

    /// <inheritdoc />
    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        await _manualControl.CloseAsync(cancellationToken);
        _circuitBreakerMonitor.Reset();
        LogCircuitReset();
    }

    private static ResiliencePipeline BuildPipeline(CircuitBreakerManualControl manualControl)
        => new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 10,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15),
                ManualControl = manualControl,
            })
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = MaxAttempts - 1,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(200),
            })
            .Build();

    [LoggerMessage(LogLevel.Warning, "AWS gateway operation failed for {ServiceKey}: {ErrorCode} {ErrorMessage}")]
    private partial void LogOperationFailed(string serviceKey, string errorCode, string errorMessage);

    [LoggerMessage(LogLevel.Information, "AWS gateway circuit breaker reset.")]
    private partial void LogCircuitReset();
}
