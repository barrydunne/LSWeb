using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteParameter;

internal sealed partial class DeleteParameterCommandValidator : AbstractValidator<DeleteParameterCommand>
{
    private readonly ILogger _logger;

    public DeleteParameterCommandValidator(ILogger<DeleteParameterCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteParameterCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteParameterCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
