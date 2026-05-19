using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SaveLambdaTestEvent;

internal sealed partial class SaveLambdaTestEventCommandValidator : AbstractValidator<SaveLambdaTestEventCommand>
{
    private readonly ILogger _logger;

    public SaveLambdaTestEventCommandValidator(ILogger<SaveLambdaTestEventCommandValidator> logger)
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

        RuleFor(_ => _.Payload)
            .NotNull();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SaveLambdaTestEventCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SaveLambdaTestEventCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
