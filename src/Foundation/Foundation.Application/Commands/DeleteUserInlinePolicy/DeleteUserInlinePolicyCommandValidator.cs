using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteUserInlinePolicy;

internal sealed partial class DeleteUserInlinePolicyCommandValidator : AbstractValidator<DeleteUserInlinePolicyCommand>
{
    private readonly ILogger _logger;

    public DeleteUserInlinePolicyCommandValidator(ILogger<DeleteUserInlinePolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.PolicyName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteUserInlinePolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteUserInlinePolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
