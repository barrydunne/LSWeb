using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetSqsRedrivePolicy;

internal sealed partial class SetSqsRedrivePolicyCommandValidator : AbstractValidator<SetSqsRedrivePolicyCommand>
{
    private const int MinMaxReceiveCount = 1;
    private const int MaxMaxReceiveCount = 1000;

    private readonly ILogger _logger;

    public SetSqsRedrivePolicyCommandValidator(ILogger<SetSqsRedrivePolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.QueueName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.DeadLetterTargetArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(arn => arn.StartsWith("arn:", StringComparison.Ordinal))
            .WithMessage("The dead-letter target must be a queue ARN.");

        RuleFor(_ => _.MaxReceiveCount)
            .InclusiveBetween(MinMaxReceiveCount, MaxMaxReceiveCount)
            .WithMessage($"Max receive count must be between {MinMaxReceiveCount} and {MaxMaxReceiveCount}.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SetSqsRedrivePolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SetSqsRedrivePolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
