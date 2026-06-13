using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutEventBridgeRule;

internal sealed partial class PutEventBridgeRuleCommandValidator : AbstractValidator<PutEventBridgeRuleCommand>
{
    private const int MaxNameLength = 64;

    private static readonly string[] _states = ["ENABLED", "DISABLED"];

    private readonly ILogger _logger;

    public PutEventBridgeRuleCommandValidator(ILogger<PutEventBridgeRuleCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength);

        RuleFor(_ => _.State)
            .Must(state => _states.Contains(state))
                .WithMessage("State must be one of 'ENABLED' or 'DISABLED'.");

        RuleFor(_ => _.EventPattern)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(BeJsonObject)
                .WithMessage("Event pattern must be a JSON object.");
    }

    private static bool BeJsonObject(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PutEventBridgeRuleCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PutEventBridgeRuleCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
