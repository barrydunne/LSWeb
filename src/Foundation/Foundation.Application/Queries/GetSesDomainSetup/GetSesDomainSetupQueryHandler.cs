using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Ses;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetSesDomainSetup;

internal sealed partial class GetSesDomainSetupQueryHandler
    : IQueryHandler<GetSesDomainSetupQuery, GetSesDomainSetupQueryResult>
{
    private readonly ISesClient _client;
    private readonly ILogger _logger;

    public GetSesDomainSetupQueryHandler(
        ISesClient client, ILogger<GetSesDomainSetupQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetSesDomainSetupQueryResult>> Handle(
        GetSesDomainSetupQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Domain);
        var setup = await _client.GetDomainSetupAsync(request.Domain, cancellationToken);
        LogHandled(setup.IsSuccess);

        if (!setup.IsSuccess)
        {
            Result<GetSesDomainSetupQueryResult> failure = setup.Error!.Value;
            return failure;
        }

        return new GetSesDomainSetupQueryResult(setup.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Getting SES domain setup for {Domain}.")]
    private partial void LogHandling(string domain);

    [LoggerMessage(LogLevel.Trace, "SES domain setup get handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
