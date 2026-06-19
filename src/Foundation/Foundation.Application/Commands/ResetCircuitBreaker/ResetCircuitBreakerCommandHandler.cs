using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Resilience;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ResetCircuitBreaker;

internal sealed partial class ResetCircuitBreakerCommandHandler : ICommandHandler<ResetCircuitBreakerCommand>
{
    private readonly ICircuitBreakerReset _reset;
    private readonly ILogger _logger;

    public ResetCircuitBreakerCommandHandler(ICircuitBreakerReset reset, ILogger<ResetCircuitBreakerCommandHandler> logger)
    {
        _reset = reset;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetCircuitBreakerCommand request, CancellationToken cancellationToken)
    {
        LogHandling();
        await _reset.ResetAsync(cancellationToken);
        LogHandled();
        return Result.Success();
    }

    [LoggerMessage(LogLevel.Trace, "Handling circuit breaker reset command.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Circuit breaker reset handled.")]
    private partial void LogHandled();
}
