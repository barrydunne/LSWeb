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

namespace Foundation.Application.Commands.RequestCertificate;

internal sealed partial class RequestCertificateCommandHandler : ICommandHandler<RequestCertificateCommand, string>
{
    private const string OperationName = "acm-request-certificate";

    private readonly ICertificateManagerClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public RequestCertificateCommandHandler(
        ICertificateManagerClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<RequestCertificateCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(RequestCertificateCommand request, CancellationToken cancellationToken)
    {
        LogHandling();
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Requesting certificate for {request.DomainName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new CertificateRequestSpecification(
            request.DomainName,
            request.ValidationMethod,
            request.SubjectAlternativeNames);

        var result = await _client.RequestCertificateAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to request certificate: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Requested certificate {result.Value}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Requesting ACM certificate.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "ACM certificate request handled.")]
    private partial void LogHandled();
}
