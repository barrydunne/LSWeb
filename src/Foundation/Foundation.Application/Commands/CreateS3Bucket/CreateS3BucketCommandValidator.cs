using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateS3Bucket;

internal sealed partial class CreateS3BucketCommandValidator : AbstractValidator<CreateS3BucketCommand>
{
    private readonly ILogger _logger;

    public CreateS3BucketCommandValidator(ILogger<CreateS3BucketCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.BucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateS3BucketCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateS3BucketCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
