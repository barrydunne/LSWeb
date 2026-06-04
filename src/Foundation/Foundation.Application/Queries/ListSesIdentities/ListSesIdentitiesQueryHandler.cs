using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Ses;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSesIdentities;

internal sealed partial class ListSesIdentitiesQueryHandler
    : IQueryHandler<ListSesIdentitiesQuery, ListSesIdentitiesQueryResult>
{
    private readonly ISesClient _client;
    private readonly ILogger _logger;

    public ListSesIdentitiesQueryHandler(
        ISesClient client, ILogger<ListSesIdentitiesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListSesIdentitiesQueryResult>> Handle(
        ListSesIdentitiesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var identities = await _client.ListIdentitiesAsync(cancellationToken);
        LogHandled(identities.IsSuccess);

        if (!identities.IsSuccess)
        {
            Result<ListSesIdentitiesQueryResult> failure = identities.Error!.Value;
            return failure;
        }

        return new ListSesIdentitiesQueryResult(identities.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing SES identities.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "SES identity list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
