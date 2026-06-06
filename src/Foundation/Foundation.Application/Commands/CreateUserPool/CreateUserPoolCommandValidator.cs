using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateUserPool;

internal sealed partial class CreateUserPoolCommandValidator : AbstractValidator<CreateUserPoolCommand>
{
    private const int MaxNameLength = 128;

    private static readonly string[] _allowedMfaModes = ["OFF", "ON", "OPTIONAL"];
    private static readonly string[] _allowedUsernameAttributes = ["email", "phone_number", "preferred_username"];
    private static readonly string[] _allowedAutoVerifiedAttributes = ["email", "phone_number"];

    private readonly ILogger _logger;

    public CreateUserPoolCommandValidator(ILogger<CreateUserPoolCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("User pool names may only contain letters, digits, spaces, and the characters '+', '=', ',', '.', '@', and '-'.");

        RuleFor(_ => _.MfaConfiguration)
            .Must(mode => _allowedMfaModes.Contains(mode))
                .WithMessage("MFA configuration must be OFF, ON, or OPTIONAL.")
            .When(_ => _.MfaConfiguration is not null);

        RuleForEach(_ => _.UsernameAttributes)
            .Must(attribute => _allowedUsernameAttributes.Contains(attribute))
                .WithMessage("Username attributes may only be email, phone_number, or preferred_username.");

        RuleForEach(_ => _.AutoVerifiedAttributes)
            .Must(attribute => _allowedAutoVerifiedAttributes.Contains(attribute))
                .WithMessage("Auto-verified attributes may only be email or phone_number.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateUserPoolCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateUserPoolCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[\w\s+=,.@-]+$")]
    private static partial Regex NamePattern();
}
