using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Ses;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetSesIdentity;

internal sealed partial class GetSesIdentityQueryHandler
    : IQueryHandler<GetSesIdentityQuery, GetSesIdentityQueryResult>
{
    private readonly ISesClient _client;
    private readonly ILogger _logger;

    public GetSesIdentityQueryHandler(
        ISesClient client, ILogger<GetSesIdentityQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetSesIdentityQueryResult>> Handle(
        GetSesIdentityQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Identity);
        var identity = await _client.GetIdentityAsync(request.Identity, cancellationToken);
        LogHandled(identity.IsSuccess);

        if (!identity.IsSuccess)
        {
            Result<GetSesIdentityQueryResult> failure = identity.Error!.Value;
            return failure;
        }

        return new GetSesIdentityQueryResult(identity.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting SES identity {Identity}.")]
    private partial void LogHandling(string identity);

    [LoggerMessage(LogLevel.Trace, "SES identity get handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
