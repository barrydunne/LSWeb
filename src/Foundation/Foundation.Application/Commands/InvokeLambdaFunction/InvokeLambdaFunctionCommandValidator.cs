using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.InvokeLambdaFunction;

internal sealed partial class InvokeLambdaFunctionCommandValidator : AbstractValidator<InvokeLambdaFunctionCommand>
{
    private readonly ILogger _logger;

    public InvokeLambdaFunctionCommandValidator(ILogger<InvokeLambdaFunctionCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.FunctionName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Payload)
            .NotNull();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<InvokeLambdaFunctionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "InvokeLambdaFunctionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
