using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Health;

/// <summary>
/// Background service that periodically probes the backend and refreshes the health snapshot.
/// The probe is fully isolated: any failure is swallowed so it affects nothing else (NFR-7.4).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Background polling loop exercised via integration tests.")]
internal sealed partial class HealthMonitor : BackgroundService
{
    private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(15);

    private readonly IBackendHealthProbe _probe;
    private readonly HealthStatusStore _store;
    private readonly ILogger _logger;

    public HealthMonitor(IBackendHealthProbe probe, HealthStatusStore store, ILogger<HealthMonitor> logger)
    {
        _probe = probe;
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PollAsync(stoppingToken);
            try
            {
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _probe.ProbeAsync(cancellationToken);
            _store.Update(HealthSnapshotBuilder.Build(result));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogPollFailed(exception);
        }
    }

    [LoggerMessage(LogLevel.Warning, "Health monitor poll failed.")]
    private partial void LogPollFailed(Exception exception);
}
