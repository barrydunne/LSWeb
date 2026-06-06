using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateUserPoolClient;

internal sealed partial class CreateUserPoolClientCommandValidator : AbstractValidator<CreateUserPoolClientCommand>
{
    private const int MaxClientNameLength = 128;

    private static readonly string[] _allowedOAuthFlows = ["code", "implicit", "client_credentials"];

    private readonly ILogger _logger;

    public CreateUserPoolClientCommandValidator(ILogger<CreateUserPoolClientCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserPoolId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ClientName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxClientNameLength)
            .Must(name => ClientNamePattern().IsMatch(name))
                .WithMessage("App client names may only contain letters, digits, spaces, and the characters '+', '=', ',', '.', '@', and '-'.");

        RuleForEach(_ => _.AllowedOAuthFlows)
            .Must(flow => _allowedOAuthFlows.Contains(flow))
                .WithMessage("Allowed OAuth flows may only be code, implicit, or client_credentials.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateUserPoolClientCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateUserPoolClientCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[\w\s+=,.@-]+$")]
    private static partial Regex ClientNamePattern();
}
