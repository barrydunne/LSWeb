using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteScheduleGroup;

internal sealed partial class DeleteScheduleGroupCommandValidator : AbstractValidator<DeleteScheduleGroupCommand>
{
    private const string DefaultGroupName = "default";

    private readonly ILogger _logger;

    public DeleteScheduleGroupCommandValidator(ILogger<DeleteScheduleGroupCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(name => !string.Equals(name, DefaultGroupName, StringComparison.Ordinal))
                .WithMessage("The default schedule group cannot be deleted.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteScheduleGroupCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteScheduleGroupCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
