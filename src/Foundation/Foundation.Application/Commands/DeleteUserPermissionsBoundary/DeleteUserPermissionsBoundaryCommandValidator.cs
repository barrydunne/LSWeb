using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteUserPermissionsBoundary;

internal sealed partial class DeleteUserPermissionsBoundaryCommandValidator : AbstractValidator<DeleteUserPermissionsBoundaryCommand>
{
    private readonly ILogger _logger;

    public DeleteUserPermissionsBoundaryCommandValidator(ILogger<DeleteUserPermissionsBoundaryCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteUserPermissionsBoundaryCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteUserPermissionsBoundaryCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
