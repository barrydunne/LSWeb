using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteLambdaFunctionUrl;

internal sealed partial class DeleteLambdaFunctionUrlCommandValidator : AbstractValidator<DeleteLambdaFunctionUrlCommand>
{
    private readonly ILogger _logger;

    public DeleteLambdaFunctionUrlCommandValidator(ILogger<DeleteLambdaFunctionUrlCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.FunctionName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<DeleteLambdaFunctionUrlCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "DeleteLambdaFunctionUrlCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
