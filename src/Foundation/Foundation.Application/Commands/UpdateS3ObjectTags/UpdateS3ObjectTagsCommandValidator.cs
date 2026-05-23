using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateS3ObjectTags;

internal sealed partial class UpdateS3ObjectTagsCommandValidator : AbstractValidator<UpdateS3ObjectTagsCommand>
{
    private readonly ILogger _logger;

    public UpdateS3ObjectTagsCommandValidator(ILogger<UpdateS3ObjectTagsCommandValidator> logger)
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

        RuleFor(_ => _.Tags)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(tags => tags.Keys.All(key => !string.IsNullOrWhiteSpace(key)))
            .WithMessage("Tag keys must not be empty.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateS3ObjectTagsCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateS3ObjectTagsCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
