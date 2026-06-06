using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteUserPoolClient;

internal sealed partial class DeleteUserPoolClientCommandValidator : AbstractValidator<DeleteUserPoolClientCommand>
{
    private readonly ILogger _logger;

    public DeleteUserPoolClientCommandValidator(ILogger<DeleteUserPoolClientCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserPoolId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ClientId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteUserPoolClientCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteUserPoolClientCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
