using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateAccessKey;

internal sealed partial class CreateAccessKeyCommandValidator : AbstractValidator<CreateAccessKeyCommand>
{
    private readonly ILogger _logger;

    public CreateAccessKeyCommandValidator(ILogger<CreateAccessKeyCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateAccessKeyCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateAccessKeyCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
