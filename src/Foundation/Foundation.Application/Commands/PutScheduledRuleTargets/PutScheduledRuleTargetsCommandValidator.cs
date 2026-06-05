using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutScheduledRuleTargets;

internal sealed partial class PutScheduledRuleTargetsCommandValidator : AbstractValidator<PutScheduledRuleTargetsCommand>
{
    private const int MaxNameLength = 64;

    private readonly ILogger _logger;

    public PutScheduledRuleTargetsCommandValidator(ILogger<PutScheduledRuleTargetsCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RuleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
            .WithMessage("RuleName may only contain letters, numbers, and the characters . - _.");

        RuleFor(_ => _.Targets)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one target is required.");

        RuleForEach(_ => _.Targets)
            .ChildRules(target =>
            {
                target.RuleFor(_ => _.Id)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                    .NotEmpty();
                target.RuleFor(_ => _.Arn)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                    .NotEmpty();
            });
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PutScheduledRuleTargetsCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PutScheduledRuleTargetsCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9_.-]+$")]
    private static partial Regex NamePattern();
}
