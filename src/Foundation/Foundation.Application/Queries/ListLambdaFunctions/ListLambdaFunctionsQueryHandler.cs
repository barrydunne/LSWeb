using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListLambdaFunctions;

internal sealed partial class ListLambdaFunctionsQueryHandler : IQueryHandler<ListLambdaFunctionsQuery, ListLambdaFunctionsQueryResult>
{
    private readonly ILambdaClient _client;
    private readonly ILogger _logger;

    public ListLambdaFunctionsQueryHandler(ILambdaClient client, ILogger<ListLambdaFunctionsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListLambdaFunctionsQueryResult>> Handle(ListLambdaFunctionsQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var functions = await _client.ListFunctionsAsync(cancellationToken);
        LogHandled(functions.IsSuccess);

        if (!functions.IsSuccess)
        {
            Result<ListLambdaFunctionsQueryResult> failure = functions.Error!.Value;
            return failure;
        }

        return new ListLambdaFunctionsQueryResult(functions.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Lambda functions.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Lambda function listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
