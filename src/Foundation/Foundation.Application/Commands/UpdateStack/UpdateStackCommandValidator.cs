using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateStack;

internal sealed partial class UpdateStackCommandValidator : AbstractValidator<UpdateStackCommand>
{
    private const int MaxNameLength = 128;

    private static readonly string[] _allowedCapabilities =
        ["CAPABILITY_IAM", "CAPABILITY_NAMED_IAM", "CAPABILITY_AUTO_EXPAND"];

    private readonly ILogger _logger;

    public UpdateStackCommandValidator(ILogger<UpdateStackCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.StackName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Stack names must begin with a letter and contain only letters, digits, and hyphens.");

        RuleFor(_ => _.TemplateBody)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleForEach(_ => _.Parameters)
            .ChildRules(parameter =>
                parameter.RuleFor(_ => _.ParameterKey)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Parameter keys must not be empty."));

        RuleForEach(_ => _.Capabilities)
            .Must(capability => _allowedCapabilities.Contains(capability))
                .WithMessage("Capabilities must be one of CAPABILITY_IAM, CAPABILITY_NAMED_IAM, or CAPABILITY_AUTO_EXPAND.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateStackCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [GeneratedRegex("^[a-zA-Z][-a-zA-Z0-9]*$")]
    private static partial Regex NamePattern();

    [LoggerMessage(LogLevel.Warning, "UpdateStackCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
