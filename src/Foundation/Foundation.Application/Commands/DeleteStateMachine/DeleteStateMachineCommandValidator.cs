using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteStateMachine;

internal sealed partial class DeleteStateMachineCommandValidator : AbstractValidator<DeleteStateMachineCommand>
{
    private readonly ILogger _logger;

    public DeleteStateMachineCommandValidator(ILogger<DeleteStateMachineCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.StateMachineArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteStateMachineCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteStateMachineCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
