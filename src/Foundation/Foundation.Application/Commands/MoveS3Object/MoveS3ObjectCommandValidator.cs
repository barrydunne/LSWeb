using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.MoveS3Object;

internal sealed partial class MoveS3ObjectCommandValidator : AbstractValidator<MoveS3ObjectCommand>
{
    private readonly ILogger _logger;

    public MoveS3ObjectCommandValidator(ILogger<MoveS3ObjectCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.SourceBucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.SourceKey)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.DestinationBucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.DestinationKey)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must((command, destinationKey) =>
                command.SourceBucketName != command.DestinationBucketName || command.SourceKey != destinationKey)
            .WithMessage("Source and destination must be different.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<MoveS3ObjectCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "MoveS3ObjectCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
