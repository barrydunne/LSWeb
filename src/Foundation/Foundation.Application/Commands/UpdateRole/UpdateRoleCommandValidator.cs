using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateRole;

internal sealed partial class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    private const int MaxNameLength = 64;
    private const int MinSessionDurationSeconds = 3600;
    private const int MaxSessionDurationSeconds = 43200;

    private readonly ILogger _logger;

    public UpdateRoleCommandValidator(ILogger<UpdateRoleCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RoleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Role names may only contain letters, digits, and the characters +=,.@_-.");

        When(_ => _.MaxSessionDuration is not null, () =>
            RuleFor(_ => _.MaxSessionDuration!.Value)
                .InclusiveBetween(MinSessionDurationSeconds, MaxSessionDurationSeconds)
                    .OverridePropertyName(nameof(UpdateRoleCommand.MaxSessionDuration))
                    .WithMessage("Maximum session duration must be between 3600 and 43200 seconds."));
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateRoleCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateRoleCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9+=,.@_-]+$")]
    private static partial Regex NamePattern();
}
