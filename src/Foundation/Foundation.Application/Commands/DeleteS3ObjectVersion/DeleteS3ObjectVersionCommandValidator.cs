using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteS3ObjectVersion;

internal sealed partial class DeleteS3ObjectVersionCommandValidator : AbstractValidator<DeleteS3ObjectVersionCommand>
{
    private readonly ILogger _logger;

    public DeleteS3ObjectVersionCommandValidator(ILogger<DeleteS3ObjectVersionCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.BucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Key)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.VersionId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteS3ObjectVersionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteS3ObjectVersionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
