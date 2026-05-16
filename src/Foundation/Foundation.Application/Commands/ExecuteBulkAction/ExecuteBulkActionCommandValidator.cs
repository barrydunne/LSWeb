using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ExecuteBulkAction;

internal sealed partial class ExecuteBulkActionCommandValidator : AbstractValidator<ExecuteBulkActionCommand>
{
    private readonly ILogger _logger;

    public ExecuteBulkActionCommandValidator(ILogger<ExecuteBulkActionCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Action)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ResourceIds)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<ExecuteBulkActionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "ExecuteBulkActionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
