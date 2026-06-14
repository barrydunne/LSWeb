using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Route53;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Route53;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteRoute53Record;

internal sealed partial class DeleteRoute53RecordCommandHandler : ICommandHandler<DeleteRoute53RecordCommand>
{
    private const string OperationName = "route53-record-delete";

    private readonly IRoute53Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public DeleteRoute53RecordCommandHandler(
        IRoute53Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<DeleteRoute53RecordCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteRoute53RecordCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name, request.Type);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting {request.Type} record {request.Name}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var record = new Route53Record(request.Name, request.Type, request.Ttl, request.Values);
        var result = await _client.DeleteRecordAsync(request.HostedZoneId, record, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete {request.Type} record {request.Name}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted {request.Type} record {request.Name}.";
        await PublishOutcomeAsync(operationId, OperationState.Succeeded, message, cancellationToken);

        LogHandled();
        return Result.Success();
    }

    private async Task PublishOutcomeAsync(string operationId, OperationState state, string message, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, state, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, state, message, DateTimeOffset.UtcNow));
    }

    [LoggerMessage(LogLevel.Trace, "Deleting Route 53 {Type} record {Name}.")]
    private partial void LogHandling(string name, string type);

    [LoggerMessage(LogLevel.Trace, "Route 53 record deletion handled.")]
    private partial void LogHandled();
}
