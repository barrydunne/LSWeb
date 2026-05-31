using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteRole;

internal sealed partial class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    private readonly ILogger _logger;

    public DeleteRoleCommandValidator(ILogger<DeleteRoleCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RoleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteRoleCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteRoleCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
