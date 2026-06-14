using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateLambdaFunctionUrl;

internal sealed partial class UpdateLambdaFunctionUrlCommandValidator : AbstractValidator<UpdateLambdaFunctionUrlCommand>
{
    private static readonly string[] _allowedAuthTypes = ["NONE", "AWS_IAM"];

    private readonly ILogger _logger;

    public UpdateLambdaFunctionUrlCommandValidator(ILogger<UpdateLambdaFunctionUrlCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.FunctionName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.AuthType)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(_allowedAuthTypes.Contains)
            .WithMessage("AuthType must be either 'NONE' or 'AWS_IAM'.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateLambdaFunctionUrlCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateLambdaFunctionUrlCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
