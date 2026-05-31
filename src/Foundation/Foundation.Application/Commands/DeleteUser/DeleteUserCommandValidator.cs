using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteUser;

internal sealed partial class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    private readonly ILogger _logger;

    public DeleteUserCommandValidator(ILogger<DeleteUserCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteUserCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteUserCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
