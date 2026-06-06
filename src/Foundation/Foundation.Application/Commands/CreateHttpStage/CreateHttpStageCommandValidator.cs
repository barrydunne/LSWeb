using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateHttpStage;

internal sealed partial class CreateHttpStageCommandValidator : AbstractValidator<CreateHttpStageCommand>
{
    private const int MaxStageNameLength = 128;

    private readonly ILogger _logger;

    public CreateHttpStageCommandValidator(ILogger<CreateHttpStageCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.ApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.StageName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxStageNameLength)
            .Matches(@"^[\w$-]+$")
                .WithMessage("Stage name may only contain letters, numbers, underscores, hyphens and the '$' character.");

        RuleFor(_ => _.DefaultRouteThrottlingBurstLimit)
            .GreaterThanOrEqualTo(0)
            .When(_ => _.DefaultRouteThrottlingBurstLimit.HasValue);

        RuleFor(_ => _.DefaultRouteThrottlingRateLimit)
            .GreaterThanOrEqualTo(0)
            .When(_ => _.DefaultRouteThrottlingRateLimit.HasValue);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateHttpStageCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateHttpStageCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
