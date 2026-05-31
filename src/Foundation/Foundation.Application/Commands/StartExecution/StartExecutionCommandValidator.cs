using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.StartExecution;

internal sealed partial class StartExecutionCommandValidator : AbstractValidator<StartExecutionCommand>
{
    private readonly ILogger _logger;

    public StartExecutionCommandValidator(ILogger<StartExecutionCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.StateMachineArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<StartExecutionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "StartExecutionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
