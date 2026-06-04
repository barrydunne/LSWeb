using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CertificateManager;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListCertificates;

internal sealed partial class ListCertificatesQueryHandler
    : IQueryHandler<ListCertificatesQuery, ListCertificatesQueryResult>
{
    private readonly ICertificateManagerClient _client;
    private readonly ILogger _logger;

    public ListCertificatesQueryHandler(
        ICertificateManagerClient client, ILogger<ListCertificatesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListCertificatesQueryResult>> Handle(
        ListCertificatesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var certificates = await _client.ListCertificatesAsync(cancellationToken);
        LogHandled(certificates.IsSuccess);

        if (!certificates.IsSuccess)
        {
            Result<ListCertificatesQueryResult> failure = certificates.Error!.Value;
            return failure;
        }

        return new ListCertificatesQueryResult(certificates.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing ACM certificates.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "ACM certificate list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
