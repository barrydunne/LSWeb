using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutSecretValue;

internal sealed partial class PutSecretValueCommandValidator : AbstractValidator<PutSecretValueCommand>
{
    private const int MaxSecretLength = 65536;

    private readonly ILogger _logger;

    public PutSecretValueCommandValidator(ILogger<PutSecretValueCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.SecretId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.SecretString)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .MaximumLength(MaxSecretLength);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<PutSecretValueCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "PutSecretValueCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
