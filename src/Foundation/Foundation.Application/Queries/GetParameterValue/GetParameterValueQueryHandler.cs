using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Diagnostics;
using Foundation.Application.Ssm;
using Foundation.Domain.Configuration;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetParameterValue;

internal sealed partial class GetParameterValueQueryHandler : IQueryHandler<GetParameterValueQuery, GetParameterValueQueryResult>
{
    private readonly ISsmClient _client;
    private readonly IRedactionService _redaction;
    private readonly ILogger _logger;

    public GetParameterValueQueryHandler(ISsmClient client, IRedactionService redaction, ILogger<GetParameterValueQueryHandler> logger)
    {
        _client = client;
        _redaction = redaction;
        _logger = logger;
    }

    public async Task<Result<GetParameterValueQueryResult>> Handle(GetParameterValueQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name, request.Reveal);
        var parameter = await _client.GetParameterValueAsync(request.Name, cancellationToken);
        LogHandled(parameter.IsSuccess);

        if (!parameter.IsSuccess)
        {
            Result<GetParameterValueQueryResult> failure = parameter.Error!.Value;
            return failure;
        }

        var isSensitive = true;
        var value = _redaction.Resolve(
            new ConfigValue(parameter.Value.Name, parameter.Value.Value, ConfigSource.Default, isSensitive),
            request.Reveal);

        return new GetParameterValueQueryResult(
            parameter.Value.Name,
            parameter.Value.Type,
            parameter.Value.Version,
            value,
            parameter.Value.Arn,
            isSensitive,
            _redaction.CanReveal);
    }

    [LoggerMessage(LogLevel.Trace, "Getting SSM parameter value for {Name}. Reveal: {Reveal}")]
    private partial void LogHandling(string name, bool reveal);

    [LoggerMessage(LogLevel.Trace, "SSM parameter value retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
