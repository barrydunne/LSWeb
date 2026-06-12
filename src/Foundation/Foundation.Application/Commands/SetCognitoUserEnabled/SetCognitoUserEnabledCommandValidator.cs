using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetCognitoUserEnabled;

internal sealed partial class SetCognitoUserEnabledCommandValidator : AbstractValidator<SetCognitoUserEnabledCommand>
{
    private readonly ILogger _logger;

    public SetCognitoUserEnabledCommandValidator(ILogger<SetCognitoUserEnabledCommandValidator> logger)
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
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SetCognitoUserEnabledCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SetCognitoUserEnabledCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
