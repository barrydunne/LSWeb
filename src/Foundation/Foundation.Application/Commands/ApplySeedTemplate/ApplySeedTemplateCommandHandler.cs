using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Seed;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Seed;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;
using ISender = MediatR.ISender;

namespace Foundation.Application.Commands.ApplySeedTemplate;

internal sealed partial class ApplySeedTemplateCommandHandler : ICommandHandler<ApplySeedTemplateCommand, SeedOutcome>
{
    private const string OperationName = "seed-template";

    private readonly ISender _sender;
    private readonly ISeedTemplateCatalogue _catalogue;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public ApplySeedTemplateCommandHandler(
        ISender sender,
        ISeedTemplateCatalogue catalogue,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<ApplySeedTemplateCommandHandler> logger)
    {
        _sender = sender;
        _catalogue = catalogue;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result<SeedOutcome>> Handle(ApplySeedTemplateCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TemplateId);

        var plan = _catalogue.GetPlan(request.TemplateId);
        if (plan is null)
        {
            LogUnknownTemplate(request.TemplateId);
            return new Error($"Unknown seed template '{request.TemplateId}'.");
        }

        var operationId = Guid.NewGuid().ToString();
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Seeding '{plan.Template.Name}' ({plan.Steps.Count} resource(s)).", DateTimeOffset.UtcNow),
            cancellationToken);

        var items = new List<SeedResourceResult>(plan.Steps.Count);
        foreach (var step in plan.Steps)
        {
            var result = await _sender.Send(step.Command, cancellationToken);
            items.Add(new SeedResourceResult(
                step.Descriptor.ServiceKey,
                step.Descriptor.ResourceType,
                step.Descriptor.Name,
                result.IsSuccess,
                result.IsSuccess ? null : result.Error!.Value.Message));
        }

        var outcome = new SeedOutcome(operationId, plan.Template.Id, items);
        var message = $"Seed '{plan.Template.Name}' completed: {outcome.SucceededCount} succeeded, {outcome.FailedCount} failed.";

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, outcome.OverallState, message, DateTimeOffset.UtcNow),
            cancellationToken);

        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, outcome.OverallState, message, DateTimeOffset.UtcNow));

        LogHandled(outcome.SucceededCount, outcome.FailedCount);
        return outcome;
    }

    [LoggerMessage(LogLevel.Trace, "Handling seed template '{TemplateId}'.")]
    private partial void LogHandling(string templateId);

    [LoggerMessage(LogLevel.Warning, "Seed template '{TemplateId}' was not found.")]
    private partial void LogUnknownTemplate(string templateId);

    [LoggerMessage(LogLevel.Trace, "Seed template handled. Succeeded: {Succeeded}, Failed: {Failed}")]
    private partial void LogHandled(int succeeded, int failed);
}
