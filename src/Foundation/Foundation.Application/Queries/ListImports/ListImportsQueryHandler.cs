using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListImports;

internal sealed partial class ListImportsQueryHandler
    : IQueryHandler<ListImportsQuery, ListImportsQueryResult>
{
    private readonly ICloudFormationClient _client;
    private readonly ILogger _logger;

    public ListImportsQueryHandler(
        ICloudFormationClient client, ILogger<ListImportsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListImportsQueryResult>> Handle(
        ListImportsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.ExportName);
        var imports = await _client.ListImportsAsync(request.ExportName, cancellationToken);
        LogHandled(imports.IsSuccess);

        if (!imports.IsSuccess)
        {
            Result<ListImportsQueryResult> failure = imports.Error!.Value;
            return failure;
        }

        return new ListImportsQueryResult(imports.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing CloudFormation imports of {ExportName}.")]
    private partial void LogHandling(string exportName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation imports handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
