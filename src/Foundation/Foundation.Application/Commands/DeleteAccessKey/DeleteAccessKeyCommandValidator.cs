using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteAccessKey;

internal sealed partial class DeleteAccessKeyCommandValidator : AbstractValidator<DeleteAccessKeyCommand>
{
    private readonly ILogger _logger;

    public DeleteAccessKeyCommandValidator(ILogger<DeleteAccessKeyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.AccessKeyId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteAccessKeyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteAccessKeyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
