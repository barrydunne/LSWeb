using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteSesIdentity;

internal sealed partial class DeleteSesIdentityCommandValidator : AbstractValidator<DeleteSesIdentityCommand>
{
    private readonly ILogger _logger;

    public DeleteSesIdentityCommandValidator(ILogger<DeleteSesIdentityCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Identity)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteSesIdentityCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteSesIdentityCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
