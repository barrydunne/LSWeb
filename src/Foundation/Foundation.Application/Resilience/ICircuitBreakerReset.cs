namespace Foundation.Application.Resilience;

/// <summary>
/// Resets the AWS gateway circuit breaker, forcing it closed so calls flow again immediately rather
/// than waiting for the break duration to elapse. Intended for operational recovery once a previously
/// unavailable downstream dependency has been restored.
/// </summary>
public interface ICircuitBreakerReset
{
    /// <summary>
    /// Close the circuit breaker and clear any recorded suspended-service state.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes once the breaker has been closed.</returns>
    Task ResetAsync(CancellationToken cancellationToken);
}
