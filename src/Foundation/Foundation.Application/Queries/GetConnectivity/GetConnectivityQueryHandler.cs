using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Configuration;
using Foundation.Application.Connectivity;
using Foundation.Domain.Connectivity;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetConnectivity;

internal sealed partial class GetConnectivityQueryHandler : IQueryHandler<GetConnectivityQuery, GetConnectivityQueryResult>
{
    private readonly IConfigProvider _configProvider;
    private readonly IConnectivityProbe _connectivityProbe;
    private readonly ILogger _logger;

    public GetConnectivityQueryHandler(
        IConfigProvider configProvider,
        IConnectivityProbe connectivityProbe,
        ILogger<GetConnectivityQueryHandler> logger)
    {
        _configProvider = configProvider;
        _connectivityProbe = connectivityProbe;
        _logger = logger;
    }

    public async Task<Result<GetConnectivityQueryResult>> Handle(GetConnectivityQuery request, CancellationToken cancellationToken)
    {
        LogHandling();

        var snapshot = _configProvider.GetSnapshot();
        var endpoint = snapshot.ServiceUrl.Value;
        var region = snapshot.Region.Value;

        var probe = await _connectivityProbe.CheckAsync(cancellationToken);

        if (probe.IsReachable)
        {
            LogConnected(endpoint, region);
            return new GetConnectivityQueryResult(new ConnectionState(ConnectivityStatus.Connected, endpoint, region, null));
        }

        LogDisconnected(endpoint, region, probe.Error);
        return new GetConnectivityQueryResult(new ConnectionState(ConnectivityStatus.Disconnected, endpoint, region, probe.Error));
    }

    [LoggerMessage(LogLevel.Trace, "Handling connectivity query.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Backend reachable at {Endpoint} in {Region}.")]
    private partial void LogConnected(string endpoint, string region);

    [LoggerMessage(LogLevel.Warning, "Backend unreachable at {Endpoint} in {Region}: {Error}")]
    private partial void LogDisconnected(string endpoint, string region, string? error);
}
