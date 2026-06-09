using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Diagnostics;
using Foundation.Application.Ssm;
using Foundation.Domain.Configuration;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetParameterHistory;

internal sealed partial class GetParameterHistoryQueryHandler : IQueryHandler<GetParameterHistoryQuery, GetParameterHistoryQueryResult>
{
    private readonly ISsmClient _client;
    private readonly IRedactionService _redaction;
    private readonly ILogger _logger;

    public GetParameterHistoryQueryHandler(ISsmClient client, IRedactionService redaction, ILogger<GetParameterHistoryQueryHandler> logger)
    {
        _client = client;
        _redaction = redaction;
        _logger = logger;
    }

    public async Task<Result<GetParameterHistoryQueryResult>> Handle(GetParameterHistoryQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name, request.Reveal);
        var history = await _client.GetParameterHistoryAsync(request.Name, cancellationToken);
        LogHandled(history.IsSuccess);

        if (!history.IsSuccess)
        {
            Result<GetParameterHistoryQueryResult> failure = history.Error!.Value;
            return failure;
        }

        var entries = history.Value.Entries
            .Select(entry =>
            {
                var isSensitive = true;
                var value = _redaction.Resolve(
                    new ConfigValue(history.Value.Name, entry.Value, ConfigSource.Default, isSensitive),
                    request.Reveal);
                return new GetParameterHistoryEntryResult(
                    entry.Type,
                    entry.Version,
                    value,
                    entry.LastModifiedDate,
                    entry.LastModifiedUser,
                    isSensitive);
            })
            .ToList();

        return new GetParameterHistoryQueryResult(history.Value.Name, _redaction.CanReveal, entries);
    }

    [LoggerMessage(LogLevel.Trace, "Getting SSM parameter history for {Name}. Reveal: {Reveal}")]
    private partial void LogHandling(string name, bool reveal);

    [LoggerMessage(LogLevel.Trace, "SSM parameter history retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
