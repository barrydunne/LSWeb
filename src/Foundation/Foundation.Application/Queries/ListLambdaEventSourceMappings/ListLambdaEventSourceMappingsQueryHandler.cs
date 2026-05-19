using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListLambdaEventSourceMappings;

internal sealed partial class ListLambdaEventSourceMappingsQueryHandler : IQueryHandler<ListLambdaEventSourceMappingsQuery, ListLambdaEventSourceMappingsQueryResult>
{
    private readonly ILambdaClient _client;
    private readonly ILogger _logger;

    public ListLambdaEventSourceMappingsQueryHandler(ILambdaClient client, ILogger<ListLambdaEventSourceMappingsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListLambdaEventSourceMappingsQueryResult>> Handle(ListLambdaEventSourceMappingsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var mappings = await _client.ListEventSourceMappingsAsync(request.FunctionName, cancellationToken);
        LogHandled(mappings.IsSuccess);

        if (!mappings.IsSuccess)
        {
            Result<ListLambdaEventSourceMappingsQueryResult> failure = mappings.Error!.Value;
            return failure;
        }

        var ordered = mappings.Value
            .OrderBy(_ => _.EventSourceArn, StringComparer.Ordinal)
            .ToList();

        return new ListLambdaEventSourceMappingsQueryResult(ordered);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Lambda event source mappings for {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda event source mapping listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
