using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetLambdaFunctionCode;

internal sealed partial class GetLambdaFunctionCodeQueryHandler : IQueryHandler<GetLambdaFunctionCodeQuery, GetLambdaFunctionCodeQueryResult>
{
    private readonly ILambdaClient _client;
    private readonly ILogger _logger;

    public GetLambdaFunctionCodeQueryHandler(ILambdaClient client, ILogger<GetLambdaFunctionCodeQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetLambdaFunctionCodeQueryResult>> Handle(GetLambdaFunctionCodeQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var code = await _client.GetFunctionCodeAsync(request.FunctionName, cancellationToken);
        LogHandled(code.IsSuccess);

        if (!code.IsSuccess)
        {
            Result<GetLambdaFunctionCodeQueryResult> failure = code.Error!.Value;
            return failure;
        }

        return new GetLambdaFunctionCodeQueryResult(code.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting Lambda function code for {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda function code retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
