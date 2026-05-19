using FluentValidation;
using FluentValidation.Results;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateLambdaFunction;

internal sealed partial class UpdateLambdaFunctionCommandValidator : AbstractValidator<UpdateLambdaFunctionCommand>
{
    private readonly ILogger _logger;

    public UpdateLambdaFunctionCommandValidator(ILogger<UpdateLambdaFunctionCommandValidator> logger)
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
            .When(_ => !string.IsNullOrEmpty(_.ZipFileBase64))
            .WithMessage("'Zip File Base64' must be a valid base64-encoded deployment package when supplied.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<UpdateLambdaFunctionCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "UpdateLambdaFunctionCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
