using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.AttachUserPolicy;

internal sealed partial class AttachUserPolicyCommandValidator : AbstractValidator<AttachUserPolicyCommand>
{
    private readonly ILogger _logger;

    public AttachUserPolicyCommandValidator(ILogger<AttachUserPolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.PolicyArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(arn => arn.StartsWith("arn:", StringComparison.Ordinal))
                .WithMessage("Policy ARN must start with 'arn:'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<AttachUserPolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "AttachUserPolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
