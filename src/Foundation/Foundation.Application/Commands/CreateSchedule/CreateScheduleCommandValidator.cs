using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateSchedule;

internal sealed partial class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
{
    private const int MaxNameLength = 64;
    private const int MinWindowMinutes = 1;
    private const int MaxWindowMinutes = 1440;

    private static readonly string[] _allowedModes = ["OFF", "FLEXIBLE"];

    private readonly ILogger _logger;

    public CreateScheduleCommandValidator(ILogger<CreateScheduleCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Schedule names may only contain letters, digits, and the characters '-', '_', and '.'.");

        RuleFor(_ => _.GroupName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ScheduleExpression)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.TargetArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.RoleArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.FlexibleTimeWindowMode)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(mode => _allowedModes.Contains(mode))
                .WithMessage("Flexible time window mode must be either OFF or FLEXIBLE.");

        RuleFor(_ => _.MaximumWindowInMinutes)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithMessage("A maximum window in minutes is required when the flexible time window mode is FLEXIBLE.")
            .InclusiveBetween(MinWindowMinutes, MaxWindowMinutes)
            .When(_ => _.FlexibleTimeWindowMode == "FLEXIBLE");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateScheduleCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateScheduleCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9_.-]+$")]
    private static partial Regex NamePattern();
}
