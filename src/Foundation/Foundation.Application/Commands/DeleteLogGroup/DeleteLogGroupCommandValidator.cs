using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteLogGroup;

internal sealed partial class DeleteLogGroupCommandValidator : AbstractValidator<DeleteLogGroupCommand>
{
    private readonly ILogger _logger;

    public DeleteLogGroupCommandValidator(ILogger<DeleteLogGroupCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.LogGroupName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteLogGroupCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteLogGroupCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
