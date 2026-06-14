using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteS3BucketPolicy;

internal sealed partial class DeleteS3BucketPolicyCommandValidator : AbstractValidator<DeleteS3BucketPolicyCommand>
{
    private readonly ILogger _logger;

    public DeleteS3BucketPolicyCommandValidator(ILogger<DeleteS3BucketPolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.BucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteS3BucketPolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteS3BucketPolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
