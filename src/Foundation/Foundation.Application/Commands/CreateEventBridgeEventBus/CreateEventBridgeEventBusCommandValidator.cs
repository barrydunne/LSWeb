using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateEventBridgeEventBus;

internal sealed partial class CreateEventBridgeEventBusCommandValidator
    : AbstractValidator<CreateEventBridgeEventBusCommand>
{
    private const int MaxNameLength = 256;

    private readonly ILogger _logger;

    public CreateEventBridgeEventBusCommandValidator(
        ILogger<CreateEventBridgeEventBusCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Event bus names may only contain letters, digits, '.', '-', and '_'.")
            .Must(name => name != "default")
                .WithMessage("The default event bus already exists and cannot be recreated.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateEventBridgeEventBusCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateEventBridgeEventBusCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[\.\-_A-Za-z0-9]+$")]
    private static partial Regex NamePattern();
}
