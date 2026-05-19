using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetLambdaEventSourceMappingState;

internal sealed partial class SetLambdaEventSourceMappingStateCommandValidator : AbstractValidator<SetLambdaEventSourceMappingStateCommand>
{
    private readonly ILogger _logger;

    public SetLambdaEventSourceMappingStateCommandValidator(ILogger<SetLambdaEventSourceMappingStateCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.FunctionName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Uuid)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SetLambdaEventSourceMappingStateCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "SetLambdaEventSourceMappingStateCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
