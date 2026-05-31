using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListAccountAliases;

internal sealed partial class ListAccountAliasesQueryHandler
    : IQueryHandler<ListAccountAliasesQuery, ListAccountAliasesQueryResult>
{
    private readonly IIamClient _client;
    private readonly ILogger _logger;

    public ListAccountAliasesQueryHandler(IIamClient client, ILogger<ListAccountAliasesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListAccountAliasesQueryResult>> Handle(
        ListAccountAliasesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var aliases = await _client.ListAccountAliasesAsync(cancellationToken);
        LogHandled(aliases.IsSuccess);

        if (!aliases.IsSuccess)
        {
            Result<ListAccountAliasesQueryResult> failure = aliases.Error!.Value;
            return failure;
        }

        return new ListAccountAliasesQueryResult(aliases.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing IAM account aliases.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "IAM account aliases retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
