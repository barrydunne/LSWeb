using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetCognitoUserPassword;

internal sealed partial class SetCognitoUserPasswordCommandValidator : AbstractValidator<SetCognitoUserPasswordCommand>
{
    private const int MinPasswordLength = 6;

    private readonly ILogger _logger;

    public SetCognitoUserPasswordCommandValidator(ILogger<SetCognitoUserPasswordCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserPoolId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Username)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Password)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MinimumLength(MinPasswordLength);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SetCognitoUserPasswordCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SetCognitoUserPasswordCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
