using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Search;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Sns;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateSnsTopic;

internal sealed partial class CreateSnsTopicCommandHandler : ICommandHandler<CreateSnsTopicCommand>
{
    private const string OperationName = "sns-create-topic";

    private readonly ISnsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateSnsTopicCommandHandler(
        ISnsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateSnsTopicCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateSnsTopicCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating {request.Name}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new SnsTopicSpecification(request.Name);

        var result = await _client.CreateTopicAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create {request.Name}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Created {request.Name}.";
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

    [LoggerMessage(LogLevel.Trace, "Creating SNS topic {Name}.")]
    private partial void LogHandling(string name);

    [LoggerMessage(LogLevel.Trace, "SNS topic create handled.")]
    private partial void LogHandled();
}
