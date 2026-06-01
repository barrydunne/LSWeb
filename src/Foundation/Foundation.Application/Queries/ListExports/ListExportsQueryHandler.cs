using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListExports;

internal sealed partial class ListExportsQueryHandler
    : IQueryHandler<ListExportsQuery, ListExportsQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public ListExportsQueryHandler(
        ICloudFormationClient client, ILogger<ListExportsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListExportsQueryResult>> Handle(
        ListExportsQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var exports = await _client.ListExportsAsync(cancellationToken);
        LogHandled(exports.IsSuccess);

        if (!exports.IsSuccess)
        {
            Result<ListExportsQueryResult> failure = exports.Error!.Value;
            return failure;
        }

        return new ListExportsQueryResult(exports.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing CloudFormation exports.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "CloudFormation exports handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
