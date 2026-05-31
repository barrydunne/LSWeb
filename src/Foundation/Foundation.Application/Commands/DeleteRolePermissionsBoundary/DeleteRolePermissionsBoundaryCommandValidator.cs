using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteRolePermissionsBoundary;

internal sealed partial class DeleteRolePermissionsBoundaryCommandValidator : AbstractValidator<DeleteRolePermissionsBoundaryCommand>
{
    private readonly ILogger _logger;

    public DeleteRolePermissionsBoundaryCommandValidator(ILogger<DeleteRolePermissionsBoundaryCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RoleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteRolePermissionsBoundaryCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteRolePermissionsBoundaryCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
