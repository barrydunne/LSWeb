using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.CertificateManager;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.CertificateManager;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ImportCertificate;

internal sealed partial class ImportCertificateCommandHandler : ICommandHandler<ImportCertificateCommand, string>
{
    private const string OperationName = "acm-import-certificate";

    private readonly ICertificateManagerClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public ImportCertificateCommandHandler(
        ICertificateManagerClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<ImportCertificateCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(ImportCertificateCommand request, CancellationToken cancellationToken)
    {
        LogHandling();
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                "Importing certificate.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new CertificateImportSpecification(
            request.Certificate,
            request.PrivateKey,
            request.CertificateChain);

        var result = await _client.ImportCertificateAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to import certificate: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Imported certificate {result.Value}.", cancellationToken);
        _searchRefresh.RequestRefresh();

        LogHandled();
        return result.Value;
    }

    private async Task PublishOutcomeAsync(string operationId, OperationState state, string message, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, state, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, state, message, DateTimeOffset.UtcNow));
    }

    [LoggerMessage(LogLevel.Trace, "Importing ACM certificate.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "ACM certificate import handled.")]
    private partial void LogHandled();
}
