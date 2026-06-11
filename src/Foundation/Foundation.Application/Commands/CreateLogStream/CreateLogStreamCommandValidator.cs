using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateLogStream;

internal sealed partial class CreateLogStreamCommandValidator : AbstractValidator<CreateLogStreamCommand>
{
    private const int MaxNameLength = 512;

    private readonly ILogger _logger;

    public CreateLogStreamCommandValidator(ILogger<CreateLogStreamCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.LogGroupName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.LogStreamName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => !name.Contains(':', StringComparison.Ordinal) && !name.Contains('*', StringComparison.Ordinal))
                .WithMessage("Log stream names may not contain ':' or '*'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateLogStreamCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateLogStreamCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
