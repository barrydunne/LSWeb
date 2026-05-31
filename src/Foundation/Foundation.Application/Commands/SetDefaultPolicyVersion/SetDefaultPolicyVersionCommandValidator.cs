using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetDefaultPolicyVersion;

internal sealed partial class SetDefaultPolicyVersionCommandValidator : AbstractValidator<SetDefaultPolicyVersionCommand>
{
    private readonly ILogger _logger;

    public SetDefaultPolicyVersionCommandValidator(ILogger<SetDefaultPolicyVersionCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.PolicyArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.VersionId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(versionId => VersionPattern().IsMatch(versionId))
                .WithMessage("Version id must be in the form v1, v2, and so on.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SetDefaultPolicyVersionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SetDefaultPolicyVersionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^v[1-9][0-9]*$")]
    private static partial Regex VersionPattern();
}
