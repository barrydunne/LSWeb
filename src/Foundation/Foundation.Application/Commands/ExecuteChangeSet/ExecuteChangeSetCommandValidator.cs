using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ExecuteChangeSet;

internal sealed partial class ExecuteChangeSetCommandValidator : AbstractValidator<ExecuteChangeSetCommand>
{
    private const int MaxNameLength = 128;

    private readonly ILogger _logger;

    public ExecuteChangeSetCommandValidator(ILogger<ExecuteChangeSetCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.StackName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Stack names must begin with a letter and contain only letters, digits, and hyphens.");

        RuleFor(_ => _.ChangeSetName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<ExecuteChangeSetCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [GeneratedRegex("^[a-zA-Z][-a-zA-Z0-9]*$")]
    private static partial Regex NamePattern();

    [LoggerMessage(LogLevel.Warning, "ExecuteChangeSetCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
