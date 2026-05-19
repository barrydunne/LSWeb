using FluentValidation;
using FluentValidation.Results;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateLambdaFunction;

internal sealed partial class CreateLambdaFunctionCommandValidator : AbstractValidator<CreateLambdaFunctionCommand>
{
    private readonly ILogger _logger;

    public CreateLambdaFunctionCommandValidator(ILogger<CreateLambdaFunctionCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.FunctionName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Runtime)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Handler)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Role)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.MemorySize)
            .GreaterThan(0);

        RuleFor(_ => _.Timeout)
            .GreaterThan(0);

        RuleFor(_ => _.ZipFileBase64)
            .Must(Base64Payload.IsValid)
            .WithMessage("'Zip File Base64' must be a non-empty base64-encoded deployment package.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<CreateLambdaFunctionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "CreateLambdaFunctionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
