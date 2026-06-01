using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DetectStackDrift;

internal sealed partial class DetectStackDriftCommandValidator : AbstractValidator<DetectStackDriftCommand>
{
    private const int MaxNameLength = 128;

    private readonly ILogger _logger;

    public DetectStackDriftCommandValidator(ILogger<DetectStackDriftCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.StackName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Stack names must begin with a letter and contain only letters, digits, and hyphens.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DetectStackDriftCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [GeneratedRegex("^[a-zA-Z][-a-zA-Z0-9]*$")]
    private static partial Regex NamePattern();

    [LoggerMessage(LogLevel.Warning, "DetectStackDriftCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
