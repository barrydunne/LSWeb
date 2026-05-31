using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteGroup;

internal sealed partial class DeleteGroupCommandValidator : AbstractValidator<DeleteGroupCommand>
{
    private readonly ILogger _logger;

    public DeleteGroupCommandValidator(ILogger<DeleteGroupCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.GroupName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteGroupCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteGroupCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
