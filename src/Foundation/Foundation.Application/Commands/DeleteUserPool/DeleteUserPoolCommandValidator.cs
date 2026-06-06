using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteUserPool;

internal sealed partial class DeleteUserPoolCommandValidator : AbstractValidator<DeleteUserPoolCommand>
{
    private readonly ILogger _logger;

    public DeleteUserPoolCommandValidator(ILogger<DeleteUserPoolCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Id)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteUserPoolCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteUserPoolCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
