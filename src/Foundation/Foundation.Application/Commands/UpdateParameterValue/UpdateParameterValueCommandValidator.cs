using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateParameterValue;

internal sealed partial class UpdateParameterValueCommandValidator : AbstractValidator<UpdateParameterValueCommand>
{
    private const int MinNameLength = 1;
    private const int MaxNameLength = 2048;
    private const int MaxValueLength = 8192;

    private readonly ILogger _logger;

    public UpdateParameterValueCommandValidator(ILogger<UpdateParameterValueCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MinimumLength(MinNameLength)
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Parameter names may only contain letters, digits, and the characters '/', '_', '.', and '-'.");

        RuleFor(_ => _.Value)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxValueLength);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateParameterValueCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateParameterValueCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9/_.-]+$")]
    private static partial Regex NamePattern();
}
