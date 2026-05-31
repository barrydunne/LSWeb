using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DetachGroupPolicy;

internal sealed partial class DetachGroupPolicyCommandValidator : AbstractValidator<DetachGroupPolicyCommand>
{
    private readonly ILogger _logger;

    public DetachGroupPolicyCommandValidator(ILogger<DetachGroupPolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.GroupName)
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
        ValidationContext<DetachGroupPolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DetachGroupPolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
