using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteLogStream;

internal sealed partial class DeleteLogStreamCommandValidator : AbstractValidator<DeleteLogStreamCommand>
{
    private readonly ILogger _logger;

    public DeleteLogStreamCommandValidator(ILogger<DeleteLogStreamCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.LogGroupName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.LogStreamName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteLogStreamCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteLogStreamCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
