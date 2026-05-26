using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateParameter;

internal sealed partial class CreateParameterCommandValidator : AbstractValidator<CreateParameterCommand>
{
    private const int MinNameLength = 1;
    private const int MaxNameLength = 2048;
    private const int MaxValueLength = 8192;
    private const int MaxDescriptionLength = 1024;

    private static readonly string[] _allowedTypes = ["String", "StringList", "SecureString"];

    private readonly ILogger _logger;

    public CreateParameterCommandValidator(ILogger<CreateParameterCommandValidator> logger)
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

        RuleFor(_ => _.Type)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(type => _allowedTypes.Contains(type))
                .WithMessage("Type must be one of String, StringList, or SecureString.");

        RuleFor(_ => _.Value)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxValueLength);

        RuleFor(_ => _.Description)
            .MaximumLength(MaxDescriptionLength)
            .When(_ => _.Description is not null);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateParameterCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateParameterCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9/_.-]+$")]
    private static partial Regex NamePattern();
}
