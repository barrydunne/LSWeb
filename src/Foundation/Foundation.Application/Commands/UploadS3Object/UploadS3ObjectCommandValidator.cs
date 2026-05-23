using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UploadS3Object;

internal sealed partial class UploadS3ObjectCommandValidator : AbstractValidator<UploadS3ObjectCommand>
{
    private readonly ILogger _logger;

    public UploadS3ObjectCommandValidator(ILogger<UploadS3ObjectCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.BucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Key)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(key => !key.EndsWith('/'))
            .WithMessage("Object keys must not end with '/'.");

        RuleFor(_ => _.Content)
            .NotNull();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UploadS3ObjectCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UploadS3ObjectCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
