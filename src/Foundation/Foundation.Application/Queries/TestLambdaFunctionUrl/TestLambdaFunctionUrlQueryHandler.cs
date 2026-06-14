using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.TestLambdaFunctionUrl;

internal sealed partial class TestLambdaFunctionUrlQueryHandler : IQueryHandler<TestLambdaFunctionUrlQuery, TestLambdaFunctionUrlQueryResult>
{
    private readonly ILambdaClient _client;
    private readonly ILogger _logger;

    public TestLambdaFunctionUrlQueryHandler(ILambdaClient client, ILogger<TestLambdaFunctionUrlQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<TestLambdaFunctionUrlQueryResult>> Handle(TestLambdaFunctionUrlQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var test = await _client.TestFunctionUrlAsync(request.FunctionName, cancellationToken);
        LogHandled(test.IsSuccess);

        if (!test.IsSuccess)
        {
            Result<TestLambdaFunctionUrlQueryResult> failure = test.Error!.Value;
            return failure;
        }

        return new TestLambdaFunctionUrlQueryResult(test.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Testing Lambda function URL for {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda function URL test handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
