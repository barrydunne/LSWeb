using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.AddUserToGroup;

internal sealed partial class AddUserToGroupCommandHandler : ICommandHandler<AddUserToGroupCommand>
{
    private const string OperationName = "iam-add-user-to-group";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public AddUserToGroupCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<AddUserToGroupCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(AddUserToGroupCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.UserName, request.GroupName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Adding {request.UserName} to {request.GroupName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.AddUserToGroupAsync(request.UserName, request.GroupName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to add {request.UserName} to {request.GroupName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Added {request.UserName} to {request.GroupName}.";
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

    [LoggerMessage(LogLevel.Trace, "Adding IAM user {UserName} to group {GroupName}.")]
    private partial void LogHandling(string userName, string groupName);

    [LoggerMessage(LogLevel.Trace, "IAM add user to group handled.")]
    private partial void LogHandled();
}
