using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateAccountAlias;

internal sealed partial class CreateAccountAliasCommandValidator : AbstractValidator<CreateAccountAliasCommand>
{
    private readonly ILogger _logger;

    public CreateAccountAliasCommandValidator(ILogger<CreateAccountAliasCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.AccountAlias)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Length(3, 63)
            .Must(_ => AliasPattern().IsMatch(_))
            .WithMessage("Account alias must be lowercase letters, digits, or hyphens, and may not start or end with a hyphen.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateAccountAliasCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateAccountAliasCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex("^[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$")]
    private static partial Regex AliasPattern();
}
