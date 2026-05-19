using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteLambdaTestEvent;

internal sealed partial class DeleteLambdaTestEventCommandValidator : AbstractValidator<DeleteLambdaTestEventCommand>
{
    private readonly ILogger _logger;

    public DeleteLambdaTestEventCommandValidator(ILogger<DeleteLambdaTestEventCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.FunctionName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Name)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteLambdaTestEventCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteLambdaTestEventCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
