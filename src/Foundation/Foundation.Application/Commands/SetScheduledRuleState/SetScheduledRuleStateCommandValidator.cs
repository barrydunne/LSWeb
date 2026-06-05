using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetScheduledRuleState;

internal sealed partial class SetScheduledRuleStateCommandValidator : AbstractValidator<SetScheduledRuleStateCommand>
{
    private const int MaxNameLength = 64;

    private readonly ILogger _logger;

    public SetScheduledRuleStateCommandValidator(ILogger<SetScheduledRuleStateCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
            .WithMessage("Name may only contain letters, numbers, and the characters . - _.");

        RuleFor(_ => _.State)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(state => state is "ENABLED" or "DISABLED")
            .WithMessage("State must be ENABLED or DISABLED.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SetScheduledRuleStateCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SetScheduledRuleStateCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9_.-]+$")]
    private static partial Regex NamePattern();
}
