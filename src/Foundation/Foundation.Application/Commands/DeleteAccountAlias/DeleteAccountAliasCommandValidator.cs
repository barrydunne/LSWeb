using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteAccountAlias;

internal sealed partial class DeleteAccountAliasCommandValidator : AbstractValidator<DeleteAccountAliasCommand>
{
    private readonly ILogger _logger;

    public DeleteAccountAliasCommandValidator(ILogger<DeleteAccountAliasCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.AccountAlias)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteAccountAliasCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteAccountAliasCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
