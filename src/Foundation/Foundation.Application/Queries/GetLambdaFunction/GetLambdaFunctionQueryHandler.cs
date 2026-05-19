using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetLambdaFunction;

internal sealed partial class GetLambdaFunctionQueryHandler : IQueryHandler<GetLambdaFunctionQuery, GetLambdaFunctionQueryResult>
{
    private readonly ILambdaClient _client;
    private readonly ILogger _logger;

    public GetLambdaFunctionQueryHandler(ILambdaClient client, ILogger<GetLambdaFunctionQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetLambdaFunctionQueryResult>> Handle(GetLambdaFunctionQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var function = await _client.GetFunctionAsync(request.FunctionName, cancellationToken);
        LogHandled(function.IsSuccess);

        if (!function.IsSuccess)
        {
            Result<GetLambdaFunctionQueryResult> failure = function.Error!.Value;
            return failure;
        }

        return new GetLambdaFunctionQueryResult(function.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting Lambda function {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda function retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
