using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UntagPolicy;

internal sealed partial class UntagPolicyCommandValidator : AbstractValidator<UntagPolicyCommand>
{
    private readonly ILogger _logger;

    public UntagPolicyCommandValidator(ILogger<UntagPolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.PolicyArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(arn => arn.StartsWith("arn:", StringComparison.Ordinal))
                .WithMessage("Policy ARN must start with 'arn:'.");

        RuleFor(_ => _.TagKeys)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleForEach(_ => _.TagKeys)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .WithMessage("Tag key must not be empty.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UntagPolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UntagPolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
