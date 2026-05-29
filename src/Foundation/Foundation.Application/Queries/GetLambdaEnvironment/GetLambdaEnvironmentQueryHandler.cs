using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Diagnostics;
using Foundation.Application.Lambda;
using Foundation.Domain.Configuration;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetLambdaEnvironment;

internal sealed partial class GetLambdaEnvironmentQueryHandler : IQueryHandler<GetLambdaEnvironmentQuery, GetLambdaEnvironmentQueryResult>
{
    private readonly ILambdaClient _client;
    private readonly IRedactionService _redaction;
    private readonly ILogger _logger;

    public GetLambdaEnvironmentQueryHandler(ILambdaClient client, IRedactionService redaction, ILogger<GetLambdaEnvironmentQueryHandler> logger)
    {
        _client = client;
        _redaction = redaction;
        _logger = logger;
    }

    public async Task<Result<GetLambdaEnvironmentQueryResult>> Handle(GetLambdaEnvironmentQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName, request.Reveal);
        var environment = await _client.GetEnvironmentAsync(request.FunctionName, cancellationToken);
        LogHandled(environment.IsSuccess);

        if (!environment.IsSuccess)
        {
            Result<GetLambdaEnvironmentQueryResult> failure = environment.Error!.Value;
            return failure;
        }

        var variables = environment.Value
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair =>
            {
                var isSensitive = LambdaEnvironmentClassifier.IsSensitive(pair.Key);
                var value = _redaction.Resolve(new ConfigValue(pair.Key, pair.Value, ConfigSource.EnvironmentVariable, isSensitive), request.Reveal);
                return new LambdaEnvironmentVariable(pair.Key, value, isSensitive);
            })
            .ToList();

        return new GetLambdaEnvironmentQueryResult(variables, _redaction.CanReveal);
    }

    [LoggerMessage(LogLevel.Trace, "Getting Lambda environment for {FunctionName}. Reveal: {Reveal}")]
    private partial void LogHandling(string functionName, bool reveal);

    [LoggerMessage(LogLevel.Trace, "Lambda environment retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
