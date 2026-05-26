using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Search;
using Foundation.Application.Ssm;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Ssm;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateParameterValue;

internal sealed partial class UpdateParameterValueCommandHandler : ICommandHandler<UpdateParameterValueCommand>
{
    private const string OperationName = "ssm-update-parameter-value";

    private readonly ISsmClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public UpdateParameterValueCommandHandler(
        ISsmClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<UpdateParameterValueCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateParameterValueCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating {request.Name}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new ParameterValueSpecification(request.Name, request.Value);

        var result = await _client.PutParameterValueAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update {request.Name}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Updated {request.Name}.";
        await PublishOutcomeAsync(operationId, OperationState.Succeeded, message, cancellationToken);
        _searchRefresh.RequestRefresh();

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

    [LoggerMessage(LogLevel.Trace, "Updating SSM parameter value for {Name}.")]
    private partial void LogHandling(string name);

    [LoggerMessage(LogLevel.Trace, "SSM parameter value update handled.")]
    private partial void LogHandled();
}
