using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateStateMachine;

internal sealed partial class CreateStateMachineCommandValidator : AbstractValidator<CreateStateMachineCommand>
{
    private static readonly HashSet<string> _types = new(StringComparer.Ordinal)
    {
        "STANDARD",
        "EXPRESS",
    };

    private readonly ILogger _logger;

    public CreateStateMachineCommandValidator(ILogger<CreateStateMachineCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(_ => _.RoleArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(arn => arn.StartsWith("arn:", StringComparison.Ordinal))
            .WithMessage("The role must be an IAM role ARN.");

        RuleFor(_ => _.Type)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(_types.Contains)
            .WithMessage("State machine type must be STANDARD or EXPRESS.");

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
        ValidationContext<CreateStateMachineCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateStateMachineCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
