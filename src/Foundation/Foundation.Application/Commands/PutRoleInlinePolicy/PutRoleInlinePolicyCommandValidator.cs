using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutRoleInlinePolicy;

internal sealed partial class PutRoleInlinePolicyCommandValidator : AbstractValidator<PutRoleInlinePolicyCommand>
{
    private readonly ILogger _logger;

    public PutRoleInlinePolicyCommandValidator(ILogger<PutRoleInlinePolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RoleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.PolicyName)
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
        ValidationContext<PutRoleInlinePolicyCommand> context, CancellationToken cancellation = default)
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

    [LoggerMessage(LogLevel.Warning, "PutRoleInlinePolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
