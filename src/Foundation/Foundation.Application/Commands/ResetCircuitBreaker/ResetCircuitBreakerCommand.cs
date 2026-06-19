using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.ResetCircuitBreaker;

/// <summary>
/// Command that resets the AWS gateway circuit breaker, closing it so calls flow again without
/// waiting for the break duration to elapse. Intended for operational recovery after a downstream
/// dependency has been restored.
/// </summary>
public record ResetCircuitBreakerCommand : ICommand;
