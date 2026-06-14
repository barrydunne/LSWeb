using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateStateMachineDefinition;

internal sealed partial class UpdateStateMachineDefinitionCommandValidator
    : AbstractValidator<UpdateStateMachineDefinitionCommand>
{
    private readonly ILogger _logger;

    public UpdateStateMachineDefinitionCommandValidator(
        ILogger<UpdateStateMachineDefinitionCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.StateMachineArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(arn => arn.StartsWith("arn:", StringComparison.Ordinal))
            .WithMessage("The state machine ARN is not valid.");

        RuleFor(_ => _.Definition)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(IsValidAslDefinition)
            .WithMessage("The definition must be a JSON document with a StartAt field and a States object.");
    }

    private static bool IsValidAslDefinition(string definition)
    {
        try
        {
            using var document = JsonDocument.Parse(definition);
            var root = document.RootElement;
            return root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("StartAt", out var startAt)
                && startAt.ValueKind == JsonValueKind.String
                && root.TryGetProperty("States", out var states)
                && states.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateStateMachineDefinitionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateStateMachineDefinitionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
