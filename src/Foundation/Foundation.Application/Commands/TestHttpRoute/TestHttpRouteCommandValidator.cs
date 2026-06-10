using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.TestHttpRoute;

internal sealed partial class TestHttpRouteCommandValidator
    : AbstractValidator<TestHttpRouteCommand>
{
    private static readonly string[] _allowedHttpMethods =
        ["GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS"];

    private readonly ILogger _logger;

    public TestHttpRouteCommandValidator(
        ILogger<TestHttpRouteCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.ApiId)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Stage)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Method)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(method => _allowedHttpMethods.Contains(method))
                .WithMessage("HTTP method must be one of GET, POST, PUT, DELETE, PATCH, HEAD or OPTIONS.");

        RuleFor(_ => _.Path)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<TestHttpRouteCommand> context,
        CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "API Gateway v2 route test invoke validation failed: {Errors}")]
    private partial void LogValidationFailure(string errors);
}
