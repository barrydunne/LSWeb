using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetS3BucketVersioning;

internal sealed partial class SetS3BucketVersioningCommandValidator : AbstractValidator<SetS3BucketVersioningCommand>
{
    private readonly ILogger _logger;

    public SetS3BucketVersioningCommandValidator(ILogger<SetS3BucketVersioningCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.BucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SetS3BucketVersioningCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SetS3BucketVersioningCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
