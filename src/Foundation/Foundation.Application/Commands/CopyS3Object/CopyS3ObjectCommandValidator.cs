using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CopyS3Object;

internal sealed partial class CopyS3ObjectCommandValidator : AbstractValidator<CopyS3ObjectCommand>
{
    private readonly ILogger _logger;

    public CopyS3ObjectCommandValidator(ILogger<CopyS3ObjectCommandValidator> logger)
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
        ValidationContext<CopyS3ObjectCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CopyS3ObjectCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
