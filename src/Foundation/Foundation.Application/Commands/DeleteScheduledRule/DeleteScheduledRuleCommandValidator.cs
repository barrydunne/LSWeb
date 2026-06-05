using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteScheduledRule;

internal sealed partial class DeleteScheduledRuleCommandValidator : AbstractValidator<DeleteScheduledRuleCommand>
{
    private const int MaxNameLength = 64;

    private readonly ILogger _logger;

    public DeleteScheduledRuleCommandValidator(ILogger<DeleteScheduledRuleCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
            .WithMessage("Name may only contain letters, numbers, and the characters . - _.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteScheduledRuleCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteScheduledRuleCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9_.-]+$")]
    private static partial Regex NamePattern();
}
