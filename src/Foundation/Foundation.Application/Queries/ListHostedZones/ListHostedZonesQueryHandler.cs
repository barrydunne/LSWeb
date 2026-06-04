using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Route53;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListHostedZones;

internal sealed partial class ListHostedZonesQueryHandler
    : IQueryHandler<ListHostedZonesQuery, ListHostedZonesQueryResult>
{
    private readonly IRoute53Client _client;
    private readonly ILogger _logger;

    public ListHostedZonesQueryHandler(
        IRoute53Client client, ILogger<ListHostedZonesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListHostedZonesQueryResult>> Handle(
        ListHostedZonesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var hostedZones = await _client.ListHostedZonesAsync(cancellationToken);
        LogHandled(hostedZones.IsSuccess);

        if (!hostedZones.IsSuccess)
        {
            Result<ListHostedZonesQueryResult> failure = hostedZones.Error!.Value;
            return failure;
        }

        return new ListHostedZonesQueryResult(hostedZones.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Route 53 hosted zones.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Route 53 hosted zone list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
