using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.TestInvokeRestMethod;

internal sealed partial class TestInvokeRestMethodCommandValidator
    : AbstractValidator<TestInvokeRestMethodCommand>
{
    private static readonly string[] _allowedHttpMethods =
        ["GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS", "ANY"];

    private readonly ILogger _logger;

    public TestInvokeRestMethodCommandValidator(
        ILogger<TestInvokeRestMethodCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RestApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ResourceId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.HttpMethod)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(method => _allowedHttpMethods.Contains(method))
                .WithMessage("HTTP method must be one of GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS or ANY.");

        RuleFor(_ => _.PathWithQueryString)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Headers)
            .NotNull();

        RuleFor(_ => _.QueryStringParameters)
            .NotNull();

        RuleFor(_ => _.StageVariables)
            .NotNull();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<TestInvokeRestMethodCommand> context,
        CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "TestInvokeRestMethodCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
