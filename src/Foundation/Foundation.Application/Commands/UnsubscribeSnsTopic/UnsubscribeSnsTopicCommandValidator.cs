using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UnsubscribeSnsTopic;

internal sealed partial class UnsubscribeSnsTopicCommandValidator : AbstractValidator<UnsubscribeSnsTopicCommand>
{
    private readonly ILogger _logger;

    public UnsubscribeSnsTopicCommandValidator(ILogger<UnsubscribeSnsTopicCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.SubscriptionArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(arn => arn.StartsWith("arn:", StringComparison.Ordinal))
            .WithMessage("A confirmed subscription ARN is required.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UnsubscribeSnsTopicCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UnsubscribeSnsTopicCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
