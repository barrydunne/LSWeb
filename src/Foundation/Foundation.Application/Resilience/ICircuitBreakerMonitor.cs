namespace Foundation.Application.Resilience;

/// <summary>
/// Records circuit-breaker transitions observed by the AWS gateway so the application can surface
/// an unmissable signal when calls to a service are being rejected by an open circuit.
/// </summary>
public interface ICircuitBreakerMonitor
{
    /// <summary>
    /// Record that a call to the given service was rejected because its circuit breaker is open.
    /// </summary>
    /// <param name="serviceKey">The catalogue service key whose call was rejected.</param>
    void RecordSuspended(string serviceKey);

    /// <summary>
    /// Record that a call to the given service succeeded, clearing any suspended state for it.
    /// </summary>
    /// <param name="serviceKey">The catalogue service key whose call succeeded.</param>
    void RecordRecovered(string serviceKey);
}
