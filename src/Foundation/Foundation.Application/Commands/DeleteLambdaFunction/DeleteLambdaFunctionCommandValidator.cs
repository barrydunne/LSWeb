using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteLambdaFunction;

internal sealed partial class DeleteLambdaFunctionCommandValidator : AbstractValidator<DeleteLambdaFunctionCommand>
{
    private readonly ILogger _logger;

    public DeleteLambdaFunctionCommandValidator(ILogger<DeleteLambdaFunctionCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.FunctionName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteLambdaFunctionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteLambdaFunctionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
