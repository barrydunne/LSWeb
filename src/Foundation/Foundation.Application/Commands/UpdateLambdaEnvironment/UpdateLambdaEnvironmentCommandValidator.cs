using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateLambdaEnvironment;

internal sealed partial class UpdateLambdaEnvironmentCommandValidator : AbstractValidator<UpdateLambdaEnvironmentCommand>
{
    private readonly ILogger _logger;

    public UpdateLambdaEnvironmentCommandValidator(ILogger<UpdateLambdaEnvironmentCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.FunctionName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Variables)
            .NotNull();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateLambdaEnvironmentCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateLambdaEnvironmentCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
