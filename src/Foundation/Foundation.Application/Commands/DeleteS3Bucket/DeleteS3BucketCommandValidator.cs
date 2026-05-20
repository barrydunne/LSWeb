using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteS3Bucket;

internal sealed partial class DeleteS3BucketCommandValidator : AbstractValidator<DeleteS3BucketCommand>
{
    private readonly ILogger _logger;

    public DeleteS3BucketCommandValidator(ILogger<DeleteS3BucketCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.BucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteS3BucketCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteS3BucketCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
