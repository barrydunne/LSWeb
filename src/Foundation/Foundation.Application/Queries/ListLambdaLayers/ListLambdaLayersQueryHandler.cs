using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListLambdaLayers;

internal sealed partial class ListLambdaLayersQueryHandler : IQueryHandler<ListLambdaLayersQuery, ListLambdaLayersQueryResult>
{
    private readonly ILambdaClient _client;
    private readonly ILogger _logger;

    public ListLambdaLayersQueryHandler(ILambdaClient client, ILogger<ListLambdaLayersQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListLambdaLayersQueryResult>> Handle(ListLambdaLayersQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var layers = await _client.ListLayersAsync(request.FunctionName, cancellationToken);
        LogHandled(layers.IsSuccess);

        if (!layers.IsSuccess)
        {
            Result<ListLambdaLayersQueryResult> failure = layers.Error!.Value;
            return failure;
        }

        var ordered = layers.Value
            .OrderBy(_ => _.Arn, StringComparer.Ordinal)
            .ToList();

        return new ListLambdaLayersQueryResult(ordered);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Lambda layers for {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda layer listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
