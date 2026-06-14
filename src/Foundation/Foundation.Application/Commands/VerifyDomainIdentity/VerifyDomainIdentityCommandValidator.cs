using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.VerifyDomainIdentity;

internal sealed partial class VerifyDomainIdentityCommandValidator : AbstractValidator<VerifyDomainIdentityCommand>
{
    private readonly ILogger _logger;

    public VerifyDomainIdentityCommandValidator(ILogger<VerifyDomainIdentityCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Domain)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(domain => domain.Contains('.', StringComparison.Ordinal)
                && !domain.Contains('@', StringComparison.Ordinal))
            .WithMessage("A valid domain name is required.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<VerifyDomainIdentityCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "VerifyDomainIdentityCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
