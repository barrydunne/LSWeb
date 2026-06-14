using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Route53;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListRoute53Records;

internal sealed partial class ListRoute53RecordsQueryHandler : IQueryHandler<ListRoute53RecordsQuery, ListRoute53RecordsQueryResult>
{
    private readonly IRoute53Client _client;
    private readonly ILogger _logger;

    public ListRoute53RecordsQueryHandler(IRoute53Client client, ILogger<ListRoute53RecordsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListRoute53RecordsQueryResult>> Handle(ListRoute53RecordsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.HostedZoneId);
        var records = await _client.ListRecordsAsync(request.HostedZoneId, cancellationToken);
        LogHandled(records.IsSuccess);

        if (!records.IsSuccess)
        {
            Result<ListRoute53RecordsQueryResult> failure = records.Error!.Value;
            return failure;
        }

        return new ListRoute53RecordsQueryResult(records.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Route 53 records for {HostedZoneId}.")]
    private partial void LogHandling(string hostedZoneId);

    [LoggerMessage(LogLevel.Trace, "Route 53 record list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
