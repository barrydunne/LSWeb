using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteRoleInlinePolicy;

internal sealed partial class DeleteRoleInlinePolicyCommandValidator : AbstractValidator<DeleteRoleInlinePolicyCommand>
{
    private readonly ILogger _logger;

    public DeleteRoleInlinePolicyCommandValidator(ILogger<DeleteRoleInlinePolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RoleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.PolicyName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteRoleInlinePolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteRoleInlinePolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
