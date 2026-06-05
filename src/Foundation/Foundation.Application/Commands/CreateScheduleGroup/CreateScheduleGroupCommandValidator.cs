using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateScheduleGroup;

internal sealed partial class CreateScheduleGroupCommandValidator : AbstractValidator<CreateScheduleGroupCommand>
{
    private const int MaxNameLength = 64;

    private readonly ILogger _logger;

    public CreateScheduleGroupCommandValidator(ILogger<CreateScheduleGroupCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxNameLength)
            .Must(name => NamePattern().IsMatch(name))
                .WithMessage("Schedule group names may only contain letters, digits, and the characters '-', '_', and '.'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateScheduleGroupCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateScheduleGroupCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);

    [GeneratedRegex(@"^[A-Za-z0-9_.-]+$")]
    private static partial Regex NamePattern();
}
