using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ChangeSqsMessageVisibility;

internal sealed partial class ChangeSqsMessageVisibilityCommandValidator
    : AbstractValidator<ChangeSqsMessageVisibilityCommand>
{
    private const int MaxVisibilityTimeoutSeconds = 43200;

    private readonly ILogger _logger;

    public ChangeSqsMessageVisibilityCommandValidator(
        ILogger<ChangeSqsMessageVisibilityCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.QueueName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ReceiptHandle)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.VisibilityTimeoutSeconds)
            .InclusiveBetween(0, MaxVisibilityTimeoutSeconds)
            .WithMessage($"Visibility timeout must be between 0 and {MaxVisibilityTimeoutSeconds} seconds.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<ChangeSqsMessageVisibilityCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "ChangeSqsMessageVisibilityCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
