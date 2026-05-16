using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Configuration;
using Foundation.Application.Connectivity;
using Foundation.Application.Diagnostics;
using Foundation.Domain.Connectivity;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetDiagnostics;

internal sealed partial class GetDiagnosticsQueryHandler : IQueryHandler<GetDiagnosticsQuery, GetDiagnosticsQueryResult>
{
    private readonly IConfigProvider _configProvider;
    private readonly IConnectivityProbe _connectivityProbe;
    private readonly IRedactionService _redactionService;
    private readonly ILogger _logger;

    public GetDiagnosticsQueryHandler(
        IConfigProvider configProvider,
        IConnectivityProbe connectivityProbe,
        IRedactionService redactionService,
        ILogger<GetDiagnosticsQueryHandler> logger)
    {
        _configProvider = configProvider;
        _connectivityProbe = connectivityProbe;
        _redactionService = redactionService;
        _logger = logger;
    }

    public async Task<Result<GetDiagnosticsQueryResult>> Handle(GetDiagnosticsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Reveal);

        var snapshot = _configProvider.GetSnapshot();
        var configuration = new[]
        {
            snapshot.AccessKey,
            snapshot.SecretKey,
            snapshot.ServiceUrl,
            snapshot.Region,
        }
        .Select(value => new DiagnosticsConfigValue(
            value.Name,
            _redactionService.Resolve(value, request.Reveal),
            value.Source.ToString(),
            value.IsSensitive))
        .ToList();

        var probe = await _connectivityProbe.CheckAsync(cancellationToken);
        var status = probe.IsReachable ? ConnectivityStatus.Connected : ConnectivityStatus.Disconnected;

        LogHandled(status, _redactionService.CanReveal);

        return new GetDiagnosticsQueryResult(
            configuration,
            snapshot.ServiceUrl.Value,
            snapshot.Region.Value,
            status.ToString(),
            probe.IsReachable ? null : probe.Error,
            _redactionService.CanReveal);
    }

    [LoggerMessage(LogLevel.Trace, "Handling diagnostics query. Reveal requested: {Reveal}")]
    private partial void LogHandling(bool reveal);

    [LoggerMessage(LogLevel.Trace, "Diagnostics query handled. Status: {Status}, RevealAllowed: {RevealAllowed}")]
    private partial void LogHandled(ConnectivityStatus status, bool revealAllowed);
}
