using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutEventBridgeEvent;

internal sealed partial class PutEventBridgeEventCommandValidator : AbstractValidator<PutEventBridgeEventCommand>
{
    private readonly ILogger _logger;

    public PutEventBridgeEventCommandValidator(ILogger<PutEventBridgeEventCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Source)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.DetailType)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Detail)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(BeJsonObject)
            .WithMessage("'Detail' must be a JSON object.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PutEventBridgeEventCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    private static bool BeJsonObject(string detail)
    {
        try
        {
            using var document = JsonDocument.Parse(detail);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    [LoggerMessage(LogLevel.Warning, "PutEventBridgeEventCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
