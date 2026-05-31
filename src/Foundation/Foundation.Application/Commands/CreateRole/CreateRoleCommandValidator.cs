using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateRole;

internal sealed partial class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    private const int MaxNameLength = 64;
    private const int MinSessionDurationSeconds = 3600;
    private const int MaxSessionDurationSeconds = 43200;

    private readonly ILogger _logger;

    public CreateRoleCommandValidator(ILogger<CreateRoleCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RoleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Role names may only contain letters, digits, and the characters +=,.@_-.");

        RuleFor(_ => _.AssumeRolePolicyDocument)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(BeJsonObject)
                .WithMessage("Trust policy document must be a JSON object.");

        When(_ => _.Path is not null, () =>
            RuleFor(_ => _.Path!)
                .Cascade(CascadeMode.Stop)
                .Must(path => PathPattern().IsMatch(path))
                    .WithMessage("Path must begin and end with a forward slash."));

        When(_ => _.MaxSessionDuration is not null, () =>
            RuleFor(_ => _.MaxSessionDuration!.Value)
                .InclusiveBetween(MinSessionDurationSeconds, MaxSessionDurationSeconds)
                    .OverridePropertyName(nameof(CreateRoleCommand.MaxSessionDuration))
                    .WithMessage("Maximum session duration must be between 3600 and 43200 seconds."));
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateRoleCommand> context, CancellationToken cancellation = default)
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

    [LoggerMessage(LogLevel.Warning, "CreateRoleCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9+=,.@_-]+$")]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"^/$|^/[\x21-\x7E]+/$")]
    private static partial Regex PathPattern();
}
