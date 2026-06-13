using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteEventBridgeEventBus;

internal sealed partial class DeleteEventBridgeEventBusCommandValidator
    : AbstractValidator<DeleteEventBridgeEventBusCommand>
{
    private readonly ILogger _logger;

    public DeleteEventBridgeEventBusCommandValidator(
        ILogger<DeleteEventBridgeEventBusCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(name => name != "default")
                .WithMessage("The default event bus cannot be deleted.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteEventBridgeEventBusCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteEventBridgeEventBusCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
