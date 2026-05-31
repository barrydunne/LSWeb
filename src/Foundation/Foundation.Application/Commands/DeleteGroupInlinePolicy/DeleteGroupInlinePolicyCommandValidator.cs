using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteGroupInlinePolicy;

internal sealed partial class DeleteGroupInlinePolicyCommandValidator : AbstractValidator<DeleteGroupInlinePolicyCommand>
{
    private readonly ILogger _logger;

    public DeleteGroupInlinePolicyCommandValidator(ILogger<DeleteGroupInlinePolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.GroupName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.PolicyName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteGroupInlinePolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteGroupInlinePolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
