using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.TagPolicy;

internal sealed partial class TagPolicyCommandValidator : AbstractValidator<TagPolicyCommand>
{
    private readonly ILogger _logger;

    public TagPolicyCommandValidator(ILogger<TagPolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.PolicyArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(arn => arn.StartsWith("arn:", StringComparison.Ordinal))
                .WithMessage("Policy ARN must start with 'arn:'.");

        RuleFor(_ => _.Tags)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleForEach(_ => _.Tags)
            .ChildRules(tag => tag.RuleFor(_ => _.Key)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty()
                .WithMessage("Tag key must not be empty."));
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<TagPolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "TagPolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
