using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetLambdaFunctionUrl;

internal sealed partial class GetLambdaFunctionUrlQueryHandler : IQueryHandler<GetLambdaFunctionUrlQuery, GetLambdaFunctionUrlQueryResult>
{
    private readonly ILambdaClient _client;
    private readonly ILogger _logger;

    public GetLambdaFunctionUrlQueryHandler(ILambdaClient client, ILogger<GetLambdaFunctionUrlQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetLambdaFunctionUrlQueryResult>> Handle(GetLambdaFunctionUrlQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var url = await _client.GetFunctionUrlAsync(request.FunctionName, cancellationToken);
        LogHandled(url.IsSuccess);

        if (!url.IsSuccess)
        {
            Result<GetLambdaFunctionUrlQueryResult> failure = url.Error!.Value;
            return failure;
        }

        return new GetLambdaFunctionUrlQueryResult(url.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting Lambda function URL for {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda function URL retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
