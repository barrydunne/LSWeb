using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.VerifyEmailIdentity;

internal sealed partial class VerifyEmailIdentityCommandValidator : AbstractValidator<VerifyEmailIdentityCommand>
{
    private readonly ILogger _logger;

    public VerifyEmailIdentityCommandValidator(ILogger<VerifyEmailIdentityCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.EmailAddress)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(email => email.Contains('@', StringComparison.Ordinal))
            .WithMessage("A valid email address is required.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<VerifyEmailIdentityCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "VerifyEmailIdentityCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
