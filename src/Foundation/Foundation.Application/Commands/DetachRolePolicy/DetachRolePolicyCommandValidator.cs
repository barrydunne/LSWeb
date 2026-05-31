using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DetachRolePolicy;

internal sealed partial class DetachRolePolicyCommandValidator : AbstractValidator<DetachRolePolicyCommand>
{
    private readonly ILogger _logger;

    public DetachRolePolicyCommandValidator(ILogger<DetachRolePolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RoleName)
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
        ValidationContext<DetachRolePolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DetachRolePolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
