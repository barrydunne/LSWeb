using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetSqsQueueAttributes;

internal sealed partial class SetSqsQueueAttributesCommandValidator : AbstractValidator<SetSqsQueueAttributesCommand>
{
    private const int MaxVisibilityTimeoutSeconds = 43200;
    private const int MinMessageRetentionPeriodSeconds = 60;
    private const int MaxMessageRetentionPeriodSeconds = 1209600;
    private const int MaxDelaySeconds = 900;
    private const int MaxReceiveMessageWaitTimeSeconds = 20;

    private readonly ILogger _logger;

    public SetSqsQueueAttributesCommandValidator(ILogger<SetSqsQueueAttributesCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.QueueName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.VisibilityTimeoutSeconds)
            .InclusiveBetween(0, MaxVisibilityTimeoutSeconds)
            .WithMessage($"Visibility timeout must be between 0 and {MaxVisibilityTimeoutSeconds} seconds.");

        RuleFor(_ => _.MessageRetentionPeriodSeconds)
            .InclusiveBetween(MinMessageRetentionPeriodSeconds, MaxMessageRetentionPeriodSeconds)
            .WithMessage(
                $"Message retention period must be between {MinMessageRetentionPeriodSeconds} and {MaxMessageRetentionPeriodSeconds} seconds.");

        RuleFor(_ => _.DelaySeconds)
            .InclusiveBetween(0, MaxDelaySeconds)
            .WithMessage($"Delay must be between 0 and {MaxDelaySeconds} seconds.");

        RuleFor(_ => _.ReceiveMessageWaitTimeSeconds)
            .InclusiveBetween(0, MaxReceiveMessageWaitTimeSeconds)
            .WithMessage($"Receive message wait time must be between 0 and {MaxReceiveMessageWaitTimeSeconds} seconds.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SetSqsQueueAttributesCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SetSqsQueueAttributesCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
