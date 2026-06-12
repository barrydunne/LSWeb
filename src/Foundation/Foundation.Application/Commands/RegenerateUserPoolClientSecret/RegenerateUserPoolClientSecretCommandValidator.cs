using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.RegenerateUserPoolClientSecret;

internal sealed partial class RegenerateUserPoolClientSecretCommandValidator : AbstractValidator<RegenerateUserPoolClientSecretCommand>
{
    private readonly ILogger _logger;

    public RegenerateUserPoolClientSecretCommandValidator(ILogger<RegenerateUserPoolClientSecretCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserPoolId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ClientId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<RegenerateUserPoolClientSecretCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "RegenerateUserPoolClientSecretCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
