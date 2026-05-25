using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateSecret;

internal sealed partial class CreateSecretCommandValidator : AbstractValidator<CreateSecretCommand>
{
    private const int MinNameLength = 1;
    private const int MaxNameLength = 512;
    private const int MaxDescriptionLength = 2048;
    private const int MaxSecretLength = 65536;

    private readonly ILogger _logger;

    public CreateSecretCommandValidator(ILogger<CreateSecretCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MinimumLength(MinNameLength)
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Secret names may only contain letters, digits, and the characters '/', '_', '+', '=', '.', '@', and '-'.");

        RuleFor(_ => _.Description)
            .MaximumLength(MaxDescriptionLength)
            .When(_ => _.Description is not null);

        RuleFor(_ => _.SecretString)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxSecretLength);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateSecretCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateSecretCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9/_+=.@-]+$")]
    private static partial Regex NamePattern();
}
