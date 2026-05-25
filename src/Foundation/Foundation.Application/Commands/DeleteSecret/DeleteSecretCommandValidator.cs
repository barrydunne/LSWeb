using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteSecret;

internal sealed partial class DeleteSecretCommandValidator : AbstractValidator<DeleteSecretCommand>
{
    private readonly ILogger _logger;

    public DeleteSecretCommandValidator(ILogger<DeleteSecretCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.SecretId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteSecretCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteSecretCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
