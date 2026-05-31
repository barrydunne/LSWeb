using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeletePolicy;

internal sealed partial class DeletePolicyCommandValidator : AbstractValidator<DeletePolicyCommand>
{
    private readonly ILogger _logger;

    public DeletePolicyCommandValidator(ILogger<DeletePolicyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.PolicyArn)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeletePolicyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeletePolicyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
