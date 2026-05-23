using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateS3Folder;

internal sealed partial class CreateS3FolderCommandValidator : AbstractValidator<CreateS3FolderCommand>
{
    private readonly ILogger _logger;

    public CreateS3FolderCommandValidator(ILogger<CreateS3FolderCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.BucketName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.FolderKey)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(key => key.EndsWith('/'))
            .WithMessage("Folder keys must end with '/'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateS3FolderCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateS3FolderCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
