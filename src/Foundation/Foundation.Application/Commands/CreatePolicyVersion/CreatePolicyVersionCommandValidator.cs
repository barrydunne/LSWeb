using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreatePolicyVersion;

internal sealed partial class CreatePolicyVersionCommandValidator : AbstractValidator<CreatePolicyVersionCommand>
{
    private readonly ILogger _logger;

    public CreatePolicyVersionCommandValidator(ILogger<CreatePolicyVersionCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.PolicyArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.PolicyDocument)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(BeJsonObject)
                .WithMessage("Policy document must be a JSON object.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreatePolicyVersionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
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

    [LoggerMessage(LogLevel.Warning, "CreatePolicyVersionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
