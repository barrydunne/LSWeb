using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateLambdaFunctionUrl;

internal sealed partial class CreateLambdaFunctionUrlCommandValidator : AbstractValidator<CreateLambdaFunctionUrlCommand>
{
    private static readonly string[] _allowedAuthTypes = ["NONE", "AWS_IAM"];

    private readonly ILogger _logger;

    public CreateLambdaFunctionUrlCommandValidator(ILogger<CreateLambdaFunctionUrlCommandValidator> logger)
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
        ValidationContext<CreateLambdaFunctionUrlCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateLambdaFunctionUrlCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
