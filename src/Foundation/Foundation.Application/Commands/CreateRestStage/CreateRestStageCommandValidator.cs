using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateRestStage;

internal sealed partial class CreateRestStageCommandValidator
    : AbstractValidator<CreateRestStageCommand>
{
    private const int MaxStageNameLength = 128;
    private const string StageNamePattern = "^[\\w-]+$";

    private readonly ILogger _logger;

    public CreateRestStageCommandValidator(
        ILogger<CreateRestStageCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RestApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.StageName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxStageNameLength)
            .Matches(StageNamePattern)
                .WithMessage("Stage name may only contain letters, numbers, underscores and hyphens.");

        RuleFor(_ => _.DeploymentId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateRestStageCommand> context,
        CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateRestStageCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
