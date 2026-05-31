using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreatePolicy;

internal sealed partial class CreatePolicyCommandValidator : AbstractValidator<CreatePolicyCommand>
{
    private const int MaxNameLength = 128;
    private const int MaxDescriptionLength = 1000;

    private readonly ILogger _logger;

    public CreatePolicyCommandValidator(ILogger<CreatePolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.PolicyName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Policy names may only contain letters, digits, and the characters +=,.@_-.");

        RuleFor(_ => _.PolicyDocument)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(BeJsonObject)
                .WithMessage("Policy document must be a JSON object.");

        When(_ => _.Path is not null, () =>
            RuleFor(_ => _.Path!)
                .Cascade(CascadeMode.Stop)
                .Must(path => PathPattern().IsMatch(path))
                    .WithMessage("Path must begin and end with a forward slash."));

        When(_ => _.Description is not null, () =>
            RuleFor(_ => _.Description!)
                .MaximumLength(MaxDescriptionLength)
                    .OverridePropertyName(nameof(CreatePolicyCommand.Description))
                    .WithMessage("Description must be 1000 characters or fewer."));
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreatePolicyCommand> context, CancellationToken cancellation = default)
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

    [LoggerMessage(LogLevel.Warning, "CreatePolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9+=,.@_-]+$")]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"^/$|^/[\x21-\x7E]+/$")]
    private static partial Regex PathPattern();
}
