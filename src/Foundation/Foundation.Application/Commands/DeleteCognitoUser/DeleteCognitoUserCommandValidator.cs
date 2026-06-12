using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteCognitoUser;

internal sealed partial class DeleteCognitoUserCommandValidator : AbstractValidator<DeleteCognitoUserCommand>
{
    private readonly ILogger _logger;

    public DeleteCognitoUserCommandValidator(ILogger<DeleteCognitoUserCommandValidator> logger)
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
        ValidationContext<DeleteCognitoUserCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteCognitoUserCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
