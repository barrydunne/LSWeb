using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateCognitoUser;

internal sealed partial class CreateCognitoUserCommandValidator : AbstractValidator<CreateCognitoUserCommand>
{
    private const int MaxUsernameLength = 128;

    private readonly ILogger _logger;

    public CreateCognitoUserCommandValidator(ILogger<CreateCognitoUserCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserPoolId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Username)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxUsernameLength);

        RuleForEach(_ => _.Attributes)
            .ChildRules(attribute => attribute
                .RuleFor(_ => _.Name)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("User attribute names must not be empty."));
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateCognitoUserCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateCognitoUserCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
