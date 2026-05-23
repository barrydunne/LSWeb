using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteS3Object;

internal sealed partial class DeleteS3ObjectCommandValidator : AbstractValidator<DeleteS3ObjectCommand>
{
    private readonly ILogger _logger;

    public DeleteS3ObjectCommandValidator(ILogger<DeleteS3ObjectCommandValidator> logger)
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
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteS3ObjectCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteS3ObjectCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
